using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matechat.util
{
    public class OpenAIEngine : IAIEngine
    {
        private readonly string _apiKey;
        private readonly string _endpoint;

        public OpenAIEngine(string apiKey, string endpoint)
        {
            _apiKey = apiKey;
            _endpoint = endpoint;
        }

        public async Task<string> SendRequestAsync(string prompt, string model = null, string systemPrompt = null)
        {
            model ??= "gpt-4"; // Default model if not specified
            systemPrompt ??= "You are an assistant."; // Default system prompt

            MelonDebug.Msg("Message Prompt: " + systemPrompt);

            using var client = new HttpClient();
            var payload = new
            {
                model,
                messages = new[]
                        {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = prompt }
              }
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await client.PostAsync(_endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            return result.choices[0].message.content;
        }


        public async Task<bool> TestConnectionAsync(string model = null)
        {
            try
            {
                model ??= Config.MODEL_NAME.Value;
                MelonLogger.Msg($"Testing connection to OpenAI with model: {model} at {_endpoint}");

                await SendRequestAsync("Test connection.", model);
                MelonLogger.Msg("OpenAI TestConnection successful.");
                return true;
            }
            catch (HttpRequestException ex)
            {
                MelonLogger.Error($"OpenAI TestConnection failed with HTTP error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"OpenAI TestConnection failed with exception: {ex.Message}");
                return false;
            }
        }
    }
}
