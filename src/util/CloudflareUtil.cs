using System.Collections;
using UnityEngine.Networking;
using System.Text;
using MelonLoader;

namespace matechat.util
{
    public static class CloudflareUtil
    {
        public static IEnumerator TestCloudflareWorker()
        {
            if (!Config.TestConfig()) yield break;

            string testMessage = CreateRequestJson("Test", "test");
            yield return SendRequest(testMessage, (success, error) =>
            {
                if (success)
                {
                    MelonLogger.Msg("Cloudflare Worker test successful! Your config is ready to use!");
                }
                else
                {
                    MelonLogger.Error($"Cloudflare Worker test failed: {error}");
                }
            });
        }

        public static IEnumerator SendCloudflareRequest(string userMessage, string systemPrompt = null)
        {
            if (!Config.TestConfig()) yield break;

            systemPrompt ??= Config.SYSTEM_PROMPT.Value;
            string requestJson = CreateRequestJson(systemPrompt, userMessage);

            UnityWebRequest webRequest = new UnityWebRequest(Config.API_URL.Value, "POST");
            try
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {Config.API_KEY.Value}");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string response = webRequest.downloadHandler.text;
                    int startIndex = response.IndexOf("\"response\":\"") + 11;
                    int endIndex = response.IndexOf("\"}", startIndex);

                    if (startIndex != -1 && endIndex != -1)
                    {
                        string aiResponse = response[startIndex..endIndex];
                        Core.GetChatFeature()?.UpdateTypingMessage(aiResponse);
                    }
                    else
                    {
                        MelonLogger.Error($"Could not find response in: {response}");
                        Core.GetChatFeature()?.UpdateTypingMessage("Sorry, I received an invalid response format.");
                    }
                }
                else
                {
                    MelonLogger.Error($"API request failed: {webRequest.error}");
                    Core.GetChatFeature()?.UpdateTypingMessage("Sorry, I couldn't connect to llm right now.");
                }
            }
            finally
            {
                if (webRequest.uploadHandler != null)
                    webRequest.uploadHandler.Dispose();
                if (webRequest.downloadHandler != null)
                    webRequest.downloadHandler.Dispose();
                webRequest.Dispose();
            }
        }

        private static string CreateRequestJson(string systemPrompt, string userMessage)
        {
            return $"{{\"messages\":[{{\"role\":\"system\",\"content\":\"{JsonUtil.EscapeJsonString(systemPrompt)}\"}},{{\"role\":\"user\",\"content\":\"{JsonUtil.EscapeJsonString(userMessage)}\"}}]}}";
        }

        private static string CreateRequestJson(string systemPrompt, string resetMessage, string userMessage)
        {
            return $"{{\"messages\":[{{\"role\":\"system\",\"content\":\"{JsonUtil.EscapeJsonString(systemPrompt)}\"}},{{\"role\":\"user\",\"content\":\"{JsonUtil.EscapeJsonString(resetMessage)}\"}},{{\"role\":\"assistant\",\"content\":\"Understood, I will use the new system prompt.\"}},{{\"role\":\"user\",\"content\":\"{JsonUtil.EscapeJsonString(userMessage)}\"}}]}}";
        }

        private static IEnumerator SendRequest(string jsonRequest, System.Action<bool, string> callback)
        {
            UnityWebRequest webRequest = new UnityWebRequest(Config.API_URL.Value, "POST");

            try
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonRequest);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {Config.API_KEY.Value}");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback(true, null);
                }
                else
                {
                    string errorMessage = webRequest.error;
                    switch (webRequest.responseCode)
                    {
                        case 401:
                        case 403:
                            errorMessage = "Authentication failed - please check your API key!";
                            break;
                        case 404:
                            errorMessage = "API URL not found - please check your account ID and endpoint!";
                            break;
                    }
                    callback(false, errorMessage);
                }
            }
            finally
            {
                if (webRequest.uploadHandler != null)
                    webRequest.uploadHandler.Dispose();
                if (webRequest.downloadHandler != null)
                    webRequest.downloadHandler.Dispose();
                webRequest.Dispose();
            }
        }
    }
}
