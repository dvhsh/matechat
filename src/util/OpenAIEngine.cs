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

        /// <summary>
        /// Uses a OpenAI/OpenRouter Endpoint
        /// </summary>
        /// <param name="apiKey">The Key provided by the User</param>
        /// <param name="endpoint">Right now the Endpoints are Hardcoded</param>

        public OpenAIEngine(string apiKey, string endpoint)
        {
            _apiKey = apiKey;
            _endpoint = endpoint;
        }

        /// <summary>
        /// Sends A Request to the AI Endpoint
        /// </summary>
        /// <param name="prompt">User Input</param>
        /// <param name="model">Selected Model</param>
        /// <param name="systemPrompt">The System Prompt</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> SendRequestAsync(string prompt, string model = null, string systemPrompt = null)
        {
            model ??= "gpt-4"; // Default model if not specified
            systemPrompt ??= "You are an assistant."; // Default system prompt

            // Start with the system message
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Retrieve and reverse context messages
            var contextMessages = Core.databaseManager.GetLastMessages(5);
            contextMessages.Reverse(); // Reverse to chronological order (oldest to newest)

            // Ensure the first non-system message is always a `user` message
            string lastRole = "system"; // Track the last role added
            foreach (var (role, message, _) in contextMessages)
            {
                if (messages.Count == 1 && role != "user")
                {
                    // Skip any initial assistant messages until the first `user` message is added
                    continue;
                }

                // Add messages while maintaining alternation
                if (role == "user" && lastRole != "user")
                {
                    messages.Add(new { role, content = message });
                    lastRole = role;
                }
                else if (role == "assistant" && lastRole != "assistant")
                {
                    messages.Add(new { role, content = message });
                    lastRole = role;
                }
            }

            // Ensure proper alternation: Add a placeholder assistant response if the last message was from `user`
            if (lastRole == "user")
            {
                messages.Add(new { role = "assistant", content = "[Placeholder: Assistant did not respond]" });
            }

            // Add the user's current input as the final message
            messages.Add(new { role = "user", content = prompt });

            // Validate that the first non-system message is a `user` message
            if (messages.Count > 1 && messages[1]?.GetType().GetProperty("role")?.GetValue(messages[1])?.ToString() != "user")
            {
                throw new InvalidOperationException("The first non-system message must be a user message.");
            }

            // Log the final payload for debugging
            var payload = new { model, messages };
            MelonLogger.Msg($"Payload: {JsonConvert.SerializeObject(payload, Formatting.Indented)}");

            // Send the request
            using var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            try
            {
                var response = await client.PostAsync(_endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (result?.choices == null || result.choices.Count == 0)
                {
                    throw new InvalidOperationException("OpenAI API response does not contain valid choices.");
                }

                // Log the user and assistant messages to the database
                Core.databaseManager.AddMessage("user", prompt);
                string assistantResponse = result.choices[0].message.content;
                Core.databaseManager.AddMessage("assistant", assistantResponse);

                return assistantResponse;
            }
            catch (Exception ex)
            {
                // Log the user's message and the error
                Core.databaseManager.AddMessage("user", prompt);
                Core.databaseManager.AddMessage("assistant", "{ERROR}");
                throw;
            }
        }

        /// <summary>
        /// Test the endpont and Model to make sure it works
        /// </summary>
        /// <param name="model">The User Selected Model</param>
        /// <returns>If test Returns Good or not</returns>
        public async Task<bool> TestConnectionAsync(string model = null)
        {
            try
            {
                // Use the configured model or default to "gpt-4"
                model ??= Config.MODEL_NAME?.Value ?? "gpt-4";

                // Test connection using the specified or configured model
                var response = await SendRequestAsync("Test connection.", model);

                if (string.IsNullOrEmpty(response))
                {
                    throw new InvalidOperationException("Test connection response is empty.");
                }

                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"OpenAI TestConnection failed with exception: {ex.Message}");
                return false;
            }
        }
    }
}
