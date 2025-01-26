using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using MelonLoader;
using Newtonsoft.Json;

namespace matechat.util
{
    public class TTSEngine : IAudioEngine
    {
        private readonly string _name;
        private readonly string _endpoint;
        private UnityWebRequest _activeRequest;

        public TTSEngine(string name, string endpoint)
        {
            _name = name;
            _endpoint = endpoint;
        }

        public IEnumerator ProcessAudio(string text, Action<string, string> callback)
        {
            _activeRequest = CreateStandardRequest(text);
            yield return _activeRequest.SendWebRequest();

            if (_activeRequest.result != UnityWebRequest.Result.Success)
            {
                callback(null, _activeRequest.error);
                yield break;
            }

            var audioData = _activeRequest.downloadHandler.data;
            var audioPath = SaveAudioFile(audioData);
            StartBackgroundPlayback(audioData);

            callback(audioPath, null);
            CleanupResources();
        }

        public async Task<string> ProcessAudioAsync(string text)
        {
            var tcs = new TaskCompletionSource<string>();
            MelonCoroutines.Start(ProcessAudioAsyncCoroutine(text, tcs));
            return await tcs.Task;
        }

        private IEnumerator ProcessAudioAsyncCoroutine(string text, TaskCompletionSource<string> tcs)
        {
            var request = CreateStandardRequest(text);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                tcs.SetException(new Exception(request.error));
                yield break;
            }

            var audioData = request.downloadHandler.data;
            var audioPath = SaveAudioFile(audioData);
            StartBackgroundPlayback(audioData);

            tcs.SetResult(audioPath);
            CleanupResources();
        }

        public void Cancel()
        {
            _activeRequest?.Abort();
            CleanupResources();
        }


        #region Core Methods
        private UnityWebRequest CreateStandardRequest(string text)
        {
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
                ["streaming_mode"] = false // TTSEngine is for non-streaming mode.
            };

            var jsonPayload = TTSRequestBuilder.BuildRequest(_name, parameters);
            var request = new UnityWebRequest(_endpoint, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            return request;
        }
        
        #endregion

        #region Audio Processing
        private void StartBackgroundPlayback(byte[] wavData)
        {
            MelonCoroutines.Start(PlayAudioFromMemory(wavData));
        }

        private IEnumerator PlayAudioFromMemory(byte[] wavData)
        {
            var (channels, sampleRate) = ParseWavHeader(wavData);
            var clip = CreateAudioClip(wavData, channels, sampleRate);

            using (var player = new TemporaryAudioPlayer(clip))
            {
                player.Play();
                while (player.IsPlaying)
                {
                    yield return null;
                }
            }
        }

        private (int channels, int sampleRate) ParseWavHeader(byte[] wavData)
        {
            return (
                BitConverter.ToInt16(wavData, 22),  // Channels
                BitConverter.ToInt32(wavData, 24)   // Sample rate
            );
        }

        private AudioClip CreateAudioClip(byte[] wavData, int channels, int sampleRate)
        {
            var samples = ConvertPcmToFloat(wavData);
            var clip = AudioClip.Create("MemoryAudio", samples.Length, channels, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private float[] ConvertPcmToFloat(byte[] wavData)
        {
            var samples = new float[(wavData.Length - 44) / 2];
            for (int i = 44, j = 0; i < wavData.Length; i += 2, j++)
            {
                samples[j] = BitConverter.ToInt16(wavData, i) / 32768f;
            }
            return samples;
        }
        #endregion

        #region Utilities
        private string SaveAudioFile(byte[] data)
        {
            var dirPath = $"UserData/TTS_{Config.AI_NAME.Value}";
            Directory.CreateDirectory(dirPath);

            var filePath = $"{dirPath}/{DateTime.Now:yyyyMMdd_HHmmss}.wav";
            File.WriteAllBytes(filePath, data);

            return filePath;
        }

        private void CleanupResources()
        {
            _activeRequest?.Dispose();
            _activeRequest = null;
        }
        #endregion
    }

    internal class TemporaryAudioPlayer : IDisposable
    {
        private readonly GameObject _playerObj;
        private readonly AudioSource _source;

        public bool IsPlaying => _source.isPlaying;

        public TemporaryAudioPlayer(AudioClip clip)
        {
            _playerObj = new GameObject("MateChatTempAudioPlayer");
            _source = _playerObj.AddComponent<AudioSource>();
            _source.clip = clip;
        }

        public void Play() => _source.Play();

        public void Dispose()
        {
            UnityEngine.Object.Destroy(_playerObj);
        }
    }
}