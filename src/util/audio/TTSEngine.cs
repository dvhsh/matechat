using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using MelonLoader;

namespace matechat.util
{
    public class TTSEngine : IAudioProcessor
    {
        public IEnumerator ProcessAudio(string text, System.Action<string, string> callback)
        {
            if (!Config.ENABLE_TTS.Value)
            {
                MelonLogger.Msg("[TTS] TTS is disabled.");
                callback(null, "TTS is disabled.");
                yield break;
            }

            string ttsApiUrl = Config.TTS_API_URL.Value;
            string requestJson = JsonRequestBuilder.CreateTTSRequest(text);
            byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson);

            UnityWebRequest webRequest = new UnityWebRequest(ttsApiUrl, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            MelonLogger.Msg($"[TTS] Sending request to {Config.TTS_ENGINE.Value} TTS server...");
            MelonLogger.Msg($"[TTS] Request JSON: {requestJson}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = webRequest.downloadHandler.data;
                MelonLogger.Msg("[TTS] Successfully received audio response.");

                yield return PlayAudioFromMemory(audioData);

                callback(null, null);
            }
            else
            {
                MelonLogger.Error($"[TTS] Error: {webRequest.error}");
                callback(null, webRequest.error);
            }
        }

        private IEnumerator PlayAudioFromMemory(byte[] audioBytes)
        {
            MelonLogger.Msg("[TTS] Decoding audio from memory...");

            WAV wav = new WAV(audioBytes);
            AudioClip clip = AudioClip.Create("TTS_Audio", wav.SampleCount, 1, wav.Frequency, false);
            clip.SetData(wav.LeftChannel, 0);

            GameObject audioObject = new GameObject("TempAudio");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();

            MelonLogger.Msg("[TTS] Playing generated voice...");

            yield return new WaitForSeconds(clip.length);
            UnityEngine.Object.Destroy(audioObject);
        }
    }
}
