using System;
using Newtonsoft.Json;

namespace matechat.util
{
    public static class JsonRequestBuilder
    {
        public static string CreateTTSRequest(string text)
        {
            switch (Config.TTS_ENGINE.Value)
            {
                case "GPT-SoVITS":
                    return JsonConvert.SerializeObject(new
                    {
                        text = text,
                        text_lang = Config.TTS_TEXT_LANG.Value,
                        ref_audio_path = Config.TTS_REF_AUDIO_PATH.Value,
                        prompt_text = Config.TTS_PROMPT_TEXT.Value,
                        prompt_lang = Config.TTS_PROMPT_LANG.Value,
                        text_split_method = Config.TTS_TEXT_SPLIT_METHOD.Value,
                        batch_size = Config.TTS_BATCH_SIZE.Value,
                        media_type = Config.TTS_MEDIA_TYPE.Value,
                        streaming_mode = Config.TTS_STREAMING_MODE.Value
                    });

                case "OpenAI":
                    return JsonConvert.SerializeObject(new
                    {
                        model = "tts-1",
                        input = text,
                        voice = "alloy"
                    });

                default:
                    throw new Exception($"[TTS] Unsupported TTS engine: {Config.TTS_ENGINE.Value}");
            }
        }

        public static string CreateAudioModelRequest(string text)
        {
            switch (Config.AUDIO_MODEL_ENGINE.Value)
            {
                case "Omni":
                    return JsonConvert.SerializeObject(new
                    {
                        text = text,
                        format = "wav"
                    });

                case "Bark":
                    return JsonConvert.SerializeObject(new
                    {
                        prompt = text,
                        voice = "neutral"
                    });

                default:
                    throw new Exception($"[AudioModel] Unsupported Audio Model engine: {Config.AUDIO_MODEL_ENGINE.Value}");
            }
        }
    }
}
