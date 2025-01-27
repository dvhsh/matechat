using System.Collections.Generic;
using Newtonsoft.Json;

namespace matechat.util
{
    public static class TTSRequestBuilder
    {
        public static string BuildRequest(string engineName, Dictionary<string, object> parameters)
        {
            return engineName.ToLower() switch
            {
                "gpt-sovits" => BuildGPTSoVITSRequest(parameters),
                //"openai-tts" => BuildOpenAITTSRequest(parameters),
                _ => throw new System.ArgumentException($"Unsupported engine: {engineName}")
            };
        }

        private static string BuildGPTSoVITSRequest(Dictionary<string, object> parameters)
        {
            var payload = new Dictionary<string, object>
            {
                ["text"] = parameters["text"],
                ["text_lang"] = parameters.GetValueOrDefault("text_lang", "zh"),
                ["ref_audio_path"] = parameters["ref_audio_path"],
                ["prompt_text"] = parameters.GetValueOrDefault("prompt_text", ""),
                ["prompt_lang"] = parameters.GetValueOrDefault("prompt_lang", "zh"),
                ["text_split_method"] = parameters.GetValueOrDefault("text_split_method", "cut5"),
                ["batch_size"] = parameters.GetValueOrDefault("batch_size", 1),
                ["media_type"] = parameters.GetValueOrDefault("media_type", "wav"),
                ["streaming_mode"] = parameters.GetValueOrDefault("streaming_mode", false)
            };
            return JsonConvert.SerializeObject(payload);
        }
    }
}