using Newtonsoft.Json;
using System.Text;

namespace matechat.util
{
    public class CloudflareWrapper
    {
        public string Id { get; private set; }
        public string Object { get; private set; }
        public long Created { get; private set; }
        public string Model { get; private set; }
        public List<Choice> Choices { get; private set; }
        public Usage Usage { get; private set; }

        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Converts a Cloudflare API response to match OpenAI's standards.
        /// </summary>
        public static CloudflareWrapper ConvertFromCloudflare(string cloudflareJsonResponse, string model)
        {
            try
            {
                var cloudflareResponse = JsonConvert.DeserializeObject<CloudflareResponse>(cloudflareJsonResponse);

                if (!cloudflareResponse.Success)
                {
                    throw new InvalidOperationException($"Cloudflare API returned errors: {string.Join(", ", cloudflareResponse.Errors)}");
                }

                var wrapper = new CloudflareWrapper
                {
                    Id = Guid.NewGuid().ToString(),
                    Object = "chat.completion",
                    Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Model = model,
                    Choices = new List<Choice>
                    {
                        new Choice
                        {
                            Index = 0,
                            Message = new Message
                            {
                                Role = "assistant",
                                Content = cloudflareResponse.Result.Response
                            },
                            FinishReason = "stop"
                        }
                    },
                    Usage = new Usage
                    {
                        PromptTokens = 0, // Placeholder: Cloudflare doesn't provide token counts
                        CompletionTokens = 0,
                        TotalTokens = 0
                    }
                };

                return wrapper;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert Cloudflare response: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends a prompt to the Cloudflare API and returns a formatted response.
        /// </summary>
        public static async Task<CloudflareWrapper> SendPromptAsync(
            string accountId,
            string apiToken,
            string model,
            string prompt,
            string endpointTemplate = "https://api.cloudflare.com/client/v4/accounts/{0}/ai/run/@cf/meta/{1}"
        )
        {
            var endpoint = string.Format(endpointTemplate, accountId, model);
            var payload = new { prompt };

            try
            {
                // Create the request
                var requestContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                HttpClient.DefaultRequestHeaders.Clear();
                HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");

                // Send the request
                var response = await HttpClient.PostAsync(endpoint, requestContent);
                response.EnsureSuccessStatusCode();

                // Read and process the response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return ConvertFromCloudflare(jsonResponse, model);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send prompt to Cloudflare API: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Serializes the wrapper object to a JSON string.
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    // Cloudflare API response structure
    public class CloudflareResponse
    {
        [JsonProperty("result")]
        public CloudflareResult Result { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }

        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
    }

    public class CloudflareResult
    {
        [JsonProperty("response")]
        public string Response { get; set; }
    }
}
