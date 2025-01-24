using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matechat.util
{
    public class AIInteraction
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public string SystemFingerprint { get; set; }
        public List<Choice> Choices { get; set; }
        public string ServiceTier { get; set; }
        public Usage Usage { get; set; }

        /// <summary>
        /// Parse a JSON response into an AIInteraction object.
        /// </summary>
        public static AIInteraction ParseFromJson(string jsonResponse)
        {
            try
            {
                return JsonConvert.DeserializeObject<AIInteraction>(jsonResponse);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse AI response: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get the assistant's message content from the first choice.
        /// </summary>
        public string GetAssistantMessage()
        {
            if (Choices != null && Choices.Count > 0)
            {
                return Choices[0]?.Message?.Content ?? string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Get detailed usage statistics from the response.
        /// </summary>
        public string GetUsageDetails()
        {
            return $"Prompt Tokens: {Usage?.PromptTokens}, Completion Tokens: {Usage?.CompletionTokens}, Total Tokens: {Usage?.TotalTokens}";
        }
    }

    public class Choice
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("logprobs")]
        public object LogProbs { get; set; }

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class Usage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonProperty("completion_tokens_details")]
        public CompletionTokensDetails CompletionTokensDetails { get; set; }
    }

    public class CompletionTokensDetails
    {
        [JsonProperty("reasoning_tokens")]
        public int ReasoningTokens { get; set; }

        [JsonProperty("accepted_prediction_tokens")]
        public int AcceptedPredictionTokens { get; set; }

        [JsonProperty("rejected_prediction_tokens")]
        public int RejectedPredictionTokens { get; set; }
    }
}
