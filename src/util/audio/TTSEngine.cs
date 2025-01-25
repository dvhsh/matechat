using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using MelonLoader;
using Newtonsoft.Json;


namespace matechat.util
{
    public class TTSEngine : IAudioProcessor
    {
        private readonly string _name;
        private readonly string _endpoint;

        public TTSEngine(string name, string endpoint)
        {
            _name = name;
            _endpoint = endpoint;
        }

        /// <summary>
        /// Unity Coroutine
        /// </summary>
        public IEnumerator ProcessAudio(string text, System.Action<string, string> callback)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            // execute `Task<string>` and wait
            MelonCoroutines.Start(ProcessAudioCoroutine(text, taskCompletionSource));

            while (!taskCompletionSource.Task.IsCompleted)
            {
                yield return null;
            }

            if (taskCompletionSource.Task.Exception != null)
            {
                callback(null, taskCompletionSource.Task.Exception.Message);
            }
            else
            {
                callback(taskCompletionSource.Task.Result, null);
            }
        }

        /// <summary>
        /// `Task<string>` await handler
        /// </summary>
        public async Task<string> ProcessAudioAsync(string text)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            MelonCoroutines.Start(ProcessAudioCoroutine(text, taskCompletionSource));

            return await taskCompletionSource.Task;
        }

        private IEnumerator ProcessAudioCoroutine(string text, TaskCompletionSource<string> tcs)
        {
            string requestJson = CreateTTSRequest(_name, text);
            byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson);

            UnityWebRequest webRequest = new UnityWebRequest(_endpoint, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            MelonLogger.Msg($"[TTS] Sending request to {_name} server...");
            MelonLogger.Msg($"[TTS] Request JSON: {requestJson}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = webRequest.downloadHandler.data;
                MelonLogger.Msg("[TTS] Successfully received audio response.");

                string audioPath = SaveWavToFile(audioData);
                tcs.SetResult(audioPath);
            }
            else
            {
                MelonLogger.Error($"[TTS] Error: {webRequest.error}");
                tcs.SetException(new Exception(webRequest.error));
            }

            webRequest.Dispose();
        }

        /// <summary>
        /// save audio data and return file path
        /// </summary>
        private string SaveWavToFile(byte[] audioData)
        {
            string directoryPath = $"UserData/TTS_{Config.AI_NAME.Value}";
            string filePath = $"{directoryPath}/{DateTime.Now:yyyyMMdd_HHmmss}.wav";
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }
            System.IO.File.WriteAllBytes(filePath, audioData);
            return filePath;
        }

        private string CreateTTSRequest(string engineName, string text)
        {
            switch (engineName.ToLower())
            {
                case "gpt-sovits":
                    var requestData = new Dictionary<string, object>
                    {
                        { "text", text },
                        { "text_lang", Config.TTS_TEXT_LANG.Value },
                        { "ref_audio_path", Config.TTS_REF_AUDIO_PATH.Value },
                        { "prompt_text", Config.TTS_PROMPT_TEXT.Value },
                        { "prompt_lang", Config.TTS_PROMPT_LANG.Value },
                        { "text_split_method", Config.TTS_TEXT_SPLIT_METHOD.Value },
                        { "batch_size", Config.TTS_BATCH_SIZE.Value },
                        { "media_type", Config.TTS_MEDIA_TYPE.Value },
                        { "streaming_mode", Config.TTS_STREAMING_MODE.Value }
                    };
                    return JsonConvert.SerializeObject(requestData);
                default:
                    throw new Exception($"{engineName} is not supported.");
            }
        }
    }
}
