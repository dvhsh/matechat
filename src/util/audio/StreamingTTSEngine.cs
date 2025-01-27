using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using MelonLoader;

namespace matechat.util
{
    public class StreamingTTSEngine : IAudioEngine
    {
        private readonly string _name;
        private readonly string _endpoint;

        // The player that will handle real-time playback.
        private IAudioStreamPlayer _player;

        // UnityWebRequest for streaming.
        private UnityWebRequest _activeRequest;

        // Internal buffers/flags for streaming process.
        private bool _isInitialized;
        private List<byte> _headerBuffer = new List<byte>();  // Temporary buffer to accumulate WAV header
        private List<byte> _fullAudioBuffer = new List<byte>(); // Accumulate all PCM data for final saving

        private int _sampleRate;
        private int _channels;

        private TaskCompletionSource<string> _tcs; // Used to return final WAV path in async scenario.

        public StreamingTTSEngine(string name, string endpoint)
        {
            _name = name;
            _endpoint = endpoint;

            // Instantiate a stream player. This is just a wrapper.
            _player = CreateStreamPlayer();
        }

        /// <summary>
        /// Coroutine-based usage: returns result via callback(string filePath, string error).
        /// </summary>
        public IEnumerator ProcessAudio(string text, Action<string, string> callback)
        {
            _activeRequest = CreateStreamingRequest(text);
            yield return _activeRequest.SendWebRequest();

            if (_activeRequest.result != UnityWebRequest.Result.Success)
            {
                callback(null, _activeRequest.error);
                yield break;
            }

            // The streaming has ended; now we create/save the final WAV file.
            string finalPath = SaveFullWavFile();
            callback(finalPath, null);
            CleanupResources();
        }

        /// <summary>
        /// Async-based usage: returns final WAV path or exception via Task.
        /// </summary>
        public async Task<string> ProcessAudioAsync(string text)
        {
            _tcs = new TaskCompletionSource<string>();
            MelonCoroutines.Start(ProcessAudioAsyncCoroutine(text, _tcs));
            return await _tcs.Task;
        }

        private IEnumerator ProcessAudioAsyncCoroutine(string text, TaskCompletionSource<string> tcs)
        {
            _activeRequest = CreateStreamingRequest(text);
            yield return _activeRequest.SendWebRequest();

            if (_activeRequest.result != UnityWebRequest.Result.Success)
            {
                tcs.SetException(new Exception(_activeRequest.error));
                CleanupResources();
                yield break;
            }

            // The streaming is done; save the final WAV file and return its path.
            string finalPath = SaveFullWavFile();
            tcs.SetResult(finalPath);
            CleanupResources();
        }

        /// <summary>
        /// Cancel the streaming if needed.
        /// </summary>
        public void Cancel()
        {
            _activeRequest?.Abort();
            _player?.Stop();
            CleanupResources();
        }

        #region Core Implementation

        private UnityWebRequest CreateStreamingRequest(string text)
        {
            // Build the JSON payload using TTSRequestBuilder or manual dictionary, etc.
            // Note: TTSRequestBuilder is assumed to be in a separate file.
            var parameters = new Dictionary<string, object>
            {
                ["text"] = text,
                ["text_lang"] = Config.TTS_TEXT_LANG.Value,
                ["ref_audio_path"] = Config.TTS_REF_AUDIO_PATH.Value,
                ["prompt_text"] = Config.TTS_PROMPT_TEXT.Value,
                ["prompt_lang"] = Config.TTS_PROMPT_LANG.Value,
                ["text_split_method"] = Config.TTS_TEXT_SPLIT_METHOD.Value,
                ["batch_size"] = Config.TTS_BATCH_SIZE.Value,
                ["media_type"] = Config.TTS_MEDIA_TYPE.Value,
                ["streaming_mode"] = true // This is crucial
            };

            string jsonPayload = TTSRequestBuilder.BuildRequest(_name, parameters);

            // POST request with custom download handler
            var request = new UnityWebRequest(_endpoint, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload)),
                downloadHandler = new StreamingDownloadHandler(OnDataReceived)
            };
            request.SetRequestHeader("Content-Type", "application/json");
            return request;
        }

        /// <summary>
        /// Create or retrieve IAudioStreamPlayer object. 
        /// // The real AudioStreamPlayer implementation is in a different file.
        /// </summary>
        private IAudioStreamPlayer CreateStreamPlayer()
        {
            var playerObj = new GameObject("StreamAudioPlayer");
            // Optionally, playerObj.hideFlags = HideFlags.HideAndDontSave;
            return new AudioStreamPlayerWrapper(playerObj);
        }

        #endregion


        #region Streaming Handling

        /// <summary>
        /// Called by StreamingDownloadHandler whenever a new chunk arrives.
        /// // Here we parse the header (44 bytes) and pass the rest to the player for real-time playback.
        /// </summary>
        private void OnDataReceived(byte[] chunk)
        {
            if (chunk == null || chunk.Length == 0)
            {
                MelonLogger.Msg("[StreamingTTSEngine] Received empty chunk.");
                return;
            }

            MelonLogger.Msg($"[StreamingTTSEngine] Received chunk size={chunk.Length}");

            try
            {
                // If not yet initialized, we must accumulate at least 44 bytes for the standard WAV header.
                if (!_isInitialized)
                {
                    _headerBuffer.AddRange(chunk);
                    if (_headerBuffer.Count >= 44)
                    {
                        // Now we can parse the WAV header from the first 44 bytes.
                        ParseWavHeader(_headerBuffer.ToArray(), out _sampleRate, out _channels);

                        // The rest (if any) is the first PCM chunk.
                        int pcmStart = 44;
                        int leftover = _headerBuffer.Count - pcmStart;
                        if (leftover > 0)
                        {
                            byte[] firstPCM = new byte[leftover];
                            Buffer.BlockCopy(_headerBuffer.ToArray(), pcmStart, firstPCM, 0, leftover);

                            // We accumulate it to fullAudioBuffer
                            _fullAudioBuffer.AddRange(firstPCM);

                            // Initialize the player and push the data for playback
                            InitializePlayer(_sampleRate, _channels);
                            _player.PushAudioData(firstPCM);
                        }
                        _isInitialized = true;
                        _headerBuffer.Clear();
                    }
                }
                else
                {
                    // Already initialized: chunk is purely PCM data
                    _fullAudioBuffer.AddRange(chunk);
                    _player.PushAudioData(chunk);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[StreamingTTSEngine] Error on streaming chunk: {ex.Message}");
                Cancel();
            }
        }

        /// <summary>
        /// Initialize the IAudioStreamPlayer with known sample rate and channels.
        /// </summary>
        private void InitializePlayer(int sampleRate, int channels)
        {
            _player.Initialize(sampleRate, channels);
            _isInitialized = true;
        }

        /// <summary>
        /// Parse the standard 44-byte WAV header to get sampleRate, channels.
        /// // In real usage, might need more robust checks for chunk sizes, etc.
        /// </summary>
        private void ParseWavHeader(byte[] header, out int sampleRate, out int channels)
        {
            if (header.Length < 44)
                throw new Exception("Invalid WAV header. Must be at least 44 bytes.");

            channels = BitConverter.ToInt16(header, 22);
            sampleRate = BitConverter.ToInt32(header, 24);
        }

        #endregion


        #region Final WAV Save

        /// <summary>
        /// Build the final .wav (header + PCM) and save it to local disk, then return the file path.
        /// </summary>
        private string SaveFullWavFile()
        {
            // If we have no PCM data or invalid rate, just skip
            if (_fullAudioBuffer.Count == 0 || _sampleRate <= 0 || _channels <= 0)
            {
                return "[No Audio Data / Streaming Failed]";
            }

            // Combine a 44-byte RIFF header with the raw PCM data
            byte[] completeWav = BuildWaveBytes(_fullAudioBuffer.ToArray(), _sampleRate, _channels);

            // Save to disk
            return SaveAudioFile(completeWav);
        }

        /// <summary>
        /// Construct a typical 44-byte PCM wav header.
        /// // 16-bit, sampleRate, channels
        /// </summary>
        private byte[] BuildWaveBytes(byte[] pcmData, int sampleRate, int channels)
        {
            using (var mem = new MemoryStream(44 + pcmData.Length))
            using (var writer = new BinaryWriter(mem))
            {
                int byteRate = sampleRate * channels * 2; // 16-bit
                int fileSize = 36 + pcmData.Length;        // (4 + (8 + Subchunk1Size) + (8 + Subchunk2Size))

                // RIFF header
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(fileSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);                   // Subchunk1Size for PCM
                writer.Write((short)1);             // AudioFormat (1 = PCM)
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)(channels * 2)); // BlockAlign
                writer.Write((short)16);             // BitsPerSample

                // data chunk
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(pcmData.Length);
                writer.Write(pcmData);

                return mem.ToArray();
            }
        }

        /// <summary>
        /// Save the completed WAV data to disk, then return its path.
        /// // TTSEngine compatibility: same folder structure, same naming, etc.
        /// </summary>
        private string SaveAudioFile(byte[] wavData)
        {
            string dirPath = $"UserData/TTS_{Config.AI_NAME.Value}";
            Directory.CreateDirectory(dirPath);

            string filePath = Path.Combine(dirPath, $"{DateTime.Now:yyyyMMdd_HHmmss}.wav");
            File.WriteAllBytes(filePath, wavData);

            MelonLogger.Msg($"[StreamingTTSEngine] Saved final WAV: {filePath}");
            return filePath;
        }

        #endregion


        private void CleanupResources()
        {
            _activeRequest?.Dispose();
            _activeRequest = null;

            _headerBuffer.Clear();
            _fullAudioBuffer.Clear();

            _player?.Stop();
            _isInitialized = false;
        }
    }
}
