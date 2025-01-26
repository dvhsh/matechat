using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MelonLoader;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace matechat.util
{
    public class StreamingTTSEngine : IAudioEngine
    {
        private readonly string _endpoint;
        private AudioStreamPlayer _player;
        private UnityWebRequest _activeRequest;

        public StreamingTTSEngine(string name, string endpoint)
        {
            _endpoint = endpoint;
            InitializePlayer();
        }

        private void InitializePlayer()
        {
            var playerObj = new GameObject("StreamAudioPlayer");
            playerObj.hideFlags = HideFlags.HideInHierarchy;
            _player = playerObj.AddComponent<AudioStreamPlayer>();
            _player.Initialize();
        }

        public IEnumerator ProcessAudio(string text, Action<string, string> callback)
        {
            var request = CreateStreamingRequest(text);
            _activeRequest = request;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                callback(null, request.error);
                yield break;
            }

            ProcessStreamResponse(request.downloadHandler.data);
            callback(SaveAudioFile(request.downloadHandler.data), null);
        }

        private UnityWebRequest CreateStreamingRequest(string text)
        {
            var payload = TTSRequestBuilder.BuildRequest("gpt-sovits", new Dictionary<string, object>
            {
                ["text"] = text,
                ["ref_audio_path"] = Config.TTS_REF_AUDIO_PATH.Value,
                ["streaming_mode"] = true
            });

            var request = new UnityWebRequest(_endpoint, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload)),
                downloadHandler = new StreamingDownloadHandler(OnChunkReceived)
            };
            request.SetRequestHeader("Content-Type", "application/json");
            return request;
        }

        private void OnChunkReceived(Il2CppStructArray<byte> chunk)
        {
            try
            {
                _player.EnqueueChunk(chunk);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Chunk processing error: {ex.Message}");
            }
        }

        private string SaveAudioFile(Il2CppStructArray<byte> data)
        {
            return "[StreamingTTSEngine] No save audio for streaming mode.";
        }

        public void Cancel()
        {
            _activeRequest?.Abort();
            _player.StopPlayback();
        }
    }

    public class StreamingDownloadHandler : DownloadHandler
    {
        private readonly Action<Il2CppStructArray<byte>> _callback;

        public StreamingDownloadHandler(Action<Il2CppStructArray<byte>> callback)
        {
            _callback = callback;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            var il2cppData = new Il2CppStructArray<byte>(dataLength);
            Buffer.BlockCopy(data, 0, il2cppData, 0, dataLength);
            _callback?.Invoke(il2cppData);
            return true;
        }
    }
}