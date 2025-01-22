using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace matechat.util
{
    public class CloudflareEngine : IAIEngine
    {
        private readonly string _apiToken;
        private readonly string _accountId;
        private readonly string _model;

        public CloudflareEngine(string apiToken, string accountId, string model)
        {
            _apiToken = apiToken;
            _accountId = accountId;
            _model = model;
        }

        public async Task<string> SendRequestAsync(string prompt, string model = null, string systemPrompt = null)
        {
            model ??= _model; // Use default model if none specified

            string endpoint = $"https://api.cloudflare.com/client/v4/accounts/{_accountId}/ai/run/@cf/meta/{model}";

            // Build the payload using the correct structure
            var payload = new
            {
                messages = new[]
                {
                    new { role = "system", content = systemPrompt ?? "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                }
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed: {response.StatusCode} - {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var wrappedResponse = CloudflareWrapper.ConvertFromCloudflare(jsonResponse, model);

            return wrappedResponse.Choices[0].Message.Content;
        }

        public async Task<bool> TestConnectionAsync(string model = null)
        {
            try
            {
                model ??= _model; // Use default model if none specified
                await SendRequestAsync("Test connection.", model, "You are a test assistant.");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
