using System.Collections;
using UnityEngine.Networking;
using System.Text;
using MelonLoader;
using matechat.util.matechat.util;
using UnityEngine;

namespace matechat.util
{
    public class OpenRouterEngine : IAIEngine
    {
        public IEnumerator SendRequest(string userMessage, string systemPrompt, System.Action<string, string> callback)
        {
            if (!Config.TestConfig())
            {
                callback(null, "Config validation failed.");
                yield break;
            }

            string requestJson = CreateRequestJson(systemPrompt, userMessage);
            string apiUrl = Config.GetAPIUrl();

            UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST");
            byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson);

            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {Config.API_KEY.Value}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                string aiResponse = ParseResponse(response);
                callback(aiResponse, null);
            }
            else
            {
                string errorMessage = HandleError(webRequest);
                callback(null, errorMessage);
            }
        }

        public IEnumerator TestEngine(System.Action<bool, string> callback)
        {
            if (!Config.TestConfig())
            {
                callback(false, "Config validation failed.");
                yield break;
            }

            string testMessage = CreateRequestJson("Test system prompt", "This is a test message to validate the configuration.");

            yield return SendRequest("This is a test message to validate the configuration.", "Test system prompt", (response, error) =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    MelonLogger.Msg("OpenRouter test successful! Your config is ready to use.");
                    callback(true, null);
                }
                else
                {
                    MelonLogger.Error($"OpenRouter test failed: {error}");
                    callback(false, error);
                }
            });
        }

        private string CreateRequestJson(string systemPrompt, string userMessage)
        {
            return $"{{" +
                   $"\"model\":\"{JsonUtil.EscapeJsonString(Config.MODEL_NAME.Value)}\"," +
                   $"\"messages\":[" +
                   $"{{\"role\":\"system\",\"content\":\"{JsonUtil.EscapeJsonString(systemPrompt)}\"}}," +
                   $"{{\"role\":\"user\",\"content\":\"{JsonUtil.EscapeJsonString(userMessage)}\"}}" +
                   $"]" +
                   $"}}";
        }

        private string ParseResponse(string jsonResponse)
        {
            MelonLogger.Msg(jsonResponse);

            try
            {

                // Use Regex to extract the "content" field in the first "message" object
                var match = System.Text.RegularExpressions.Regex.Match(jsonResponse, "\"content\":\"(.*?)\"");
                if (match.Success)
                {
                    return match.Groups[1].Value.Replace("\\n", "\n").Replace("\\\"", "\"");
                }

                return "Error: Unable to find content in the response.";
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to parse response: {ex.Message}");
                return "Error: Unable to parse response.";
            }
        }


        private string HandleError(UnityWebRequest webRequest)
        {
            string errorMessage = webRequest.error;
            string responseText = webRequest.downloadHandler?.text;

            MelonLogger.Error($"Request failed: {errorMessage}. Response: {responseText}");

            return errorMessage;
        }
    }

    [System.Serializable]
    public class TopLevelResponse
    {
        public Choice[] choices; // Top-level array of choices
    }

    [System.Serializable]
    public class Choice
    {
        public Message message; // Nested message object
    }

    [System.Serializable]
    public class Message
    {
        public string content; // Actual content from the assistant
    }
}
