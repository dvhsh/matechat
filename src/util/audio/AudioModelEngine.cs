using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using MelonLoader;

namespace matechat.util
{
    public class AudioModelEngine : IAudioProcessor
    {
        public IEnumerator ProcessAudio(string text, System.Action<string, string> callback)
        {
            if (!Config.ENABLE_AUDIO_MODEL.Value)
            {
                MelonLogger.Msg("[AudioModel] Audio model is disabled.");
                callback(null, "Audio model is disabled.");
                yield break;
            }

            string audioModelUrl = Config.AUDIO_MODEL_API_URL.Value;
            string requestJson = JsonRequestBuilder.CreateAudioModelRequest(text);
            byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson);

            UnityWebRequest webRequest = new UnityWebRequest(audioModelUrl, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            MelonLogger.Msg("[AudioModel] Sending request to audio model...");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = webRequest.downloadHandler.data;
                string audioPath = "UserData/audio_output.wav";
                File.WriteAllBytes(audioPath, audioData);

                MelonLogger.Msg($"[AudioModel] Saved audio to {audioPath}. Playing...");

                yield return PlayAudio(audioPath);

                callback(audioPath, null);
            }
            else
            {
                MelonLogger.Error($"[AudioModel] Error: {webRequest.error}");
                callback(null, webRequest.error);
            }
        }

        private IEnumerator PlayAudio(string path)
        {
            UnityWebRequest audioLoader = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV);

            try
            {
                yield return audioLoader.SendWebRequest();

                if (audioLoader.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(audioLoader);
                    GameObject audioObject = new GameObject("TempAudio");
                    AudioSource audioSource = audioObject.AddComponent<AudioSource>();
                    audioSource.clip = clip;
                    audioSource.Play();

                    MelonLogger.Msg("[AudioModel] Playing generated audio...");

                    yield return new WaitForSeconds(clip.length);
                    UnityEngine.Object.Destroy(audioObject);
                }
                else
                {
                    MelonLogger.Error($"[AudioModel] Failed to load audio file: {audioLoader.error}");
                }
            }
            finally
            {
                audioLoader.Dispose();
            }
        }
    }
}