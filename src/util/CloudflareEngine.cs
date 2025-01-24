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

        /// <summary>
        /// Initilialize the Engine to Use Cloudflare
        /// </summary>
        /// <param name="apiToken">Provided Token from Cloud Flare</param>
        /// <param name="accountId">Account ID from Cloud Flare</param>
        /// <param name="model">Selected model from the endpoint</param>
        public CloudflareEngine(string apiToken, string accountId, string model)
        {
            _apiToken = apiToken;
            _accountId = accountId;
            _model = model;
        }

        /// <summary>
        /// Send request to Endpoint
        /// </summary>
        /// <param name="prompt">User Input</param>
        /// <param name="model">Selected Model</param>
        /// <param name="systemPrompt">System Prompt</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> SendRequestAsync(string prompt, string model = null, string systemPrompt = null)
        {
            model ??= _model; // Use default model if none specified
            string endpoint = $"https://api.cloudflare.com/client/v4/accounts/{_accountId}/ai/run/@cf/meta/{model}";




            // Start with the system message
            var payloadMessages = new List<object>
            {
                new { role = "system", content = systemPrompt ?? "You are a helpful assistant." }
            };

            // Retrieve and reverse context messages
            var contextMessages = Core.databaseManager.GetLastMessages(5);
            contextMessages.Reverse(); // Reverse to chronological order (oldest to newest)

            // Ensure the first non-system message is always a `user` message
            string lastRole = "system"; // Track the last role added
            foreach (var (role, message, _) in contextMessages)
            {
                if (payloadMessages.Count == 1 && role != "user")
                {
                    // Skip any initial assistant messages until the first `user` message is added
                    continue;
                }

                // Add messages while maintaining alternation
                if (role == "user" && lastRole != "user")
                {
                    payloadMessages.Add(new { role, content = message });
                    lastRole = role;
                }
                else if (role == "assistant" && lastRole != "assistant")
                {
                    payloadMessages.Add(new { role, content = message });
                    lastRole = role;
                }
            }

            // Ensure proper alternation: Add a placeholder assistant response if the last message was from `user`
            if (lastRole == "user")
            {
                payloadMessages.Add(new { role = "assistant", content = "[Placeholder: Assistant did not respond]" });
            }

            // Add the user's current input as the final message
            payloadMessages.Add(new { role = "user", content = prompt });

            // Validate that the first non-system message is a `user` message
            if (payloadMessages.Count > 1 && payloadMessages[1]?.GetType().GetProperty("role")?.GetValue(payloadMessages[1])?.ToString() != "user")
            {
                throw new InvalidOperationException("The first non-system message must be a user message.");
            }

            // Create the final payload
            var payload = new { messages = payloadMessages };


            // Send the request
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");

            try
            {
                var response = await client.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Request failed: {response.StatusCode} - {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Process the response using the Cloudflare wrapper
                var wrappedResponse = CloudflareWrapper.ConvertFromCloudflare(jsonResponse, model);

                // Log the user and assistant messages to the database
                Core.databaseManager.AddMessage("user", prompt);
                string assistantResponse = wrappedResponse.Choices[0].Message.Content;
                Core.databaseManager.AddMessage("assistant", assistantResponse);
                return assistantResponse;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error during Cloudflare request: {ex.Message}");
                throw;
            }
        }



        public async Task<bool> TestConnectionAsync(string model = null)
        {
            try
            {
                model ??= _model; // Use default model if none specified
                await SendRequestAsync("Test connection.", model, "You are a test assistant. Just Write 1.");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
