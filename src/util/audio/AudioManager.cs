using System.Collections;
using MelonLoader;

namespace matechat.util
{
    public static class AudioManager
    {
        private static IAudioProcessor audioEngine;

        public static void Initialize()
        {
            if (Config.ENABLE_AUDIO_MODEL.Value)
            {
                audioEngine = new AudioModelEngine();
                MelonLogger.Msg("[Audio] Using Audio Model Processor.");
            }
            else
            {
                audioEngine = new TTSEngine();
                MelonLogger.Msg("[Audio] Using TTS Processor.");
            }
        }

        public static IEnumerator PlayAudio(string text, System.Action<string, string> callback)
        {
            if (Core.GetAudioEngine() == null)
            {
                MelonLogger.Error("[Audio] AudioProcessor is not initialized.");
                callback(null, "AudioProcessor is not initialized.");
                yield break;
            }

            MelonLogger.Msg("[Audio] Calling ProcessAudio with text: " + text);
            yield return Core.GetAudioEngine().ProcessAudio(text, callback);
        }
    }
  }
