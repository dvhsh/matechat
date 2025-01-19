using System.Collections;
using UnityEngine.Networking;
using System.Text;
using MelonLoader;

namespace matechat.util
{
    public class CloudflareUtil : IAIEngine
    {
        public IEnumerator SendRequest(string userMessage, string systemPrompt,
                                       System.Action<string, string> callback)
        {
            if (!Config.TestConfig())
            {
                callback(null, "Config validation failed.");
                yield break;
            }

            systemPrompt ??= Config.SYSTEM_PROMPT.Value;
            string requestJson = CreateRequestJson(systemPrompt, userMessage);

            UnityWebRequest webRequest =
                new UnityWebRequest(Config.GetAPIUrl(), "POST");
            byte[] jsonToSend = Encoding.UTF8.GetBytes(requestJson);

            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization",
                                        $"Bearer {Config.API_KEY.Value}");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string rawResponse = webRequest.downloadHandler.text;
                string parsedResponse = ParseCloudflareResponse(rawResponse);
                callback(parsedResponse, null);
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
                        errorMessage =
                            "API URL not found - please check your account ID and endpoint!";
                        break;
                }

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

            string testMessage = CreateRequestJson("This is a system test prompt.",
                                                   "This is a user test message.");

            yield return SendRequest("Test", "test", (response, error) =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    MelonLogger.Msg(
                        "Cloudflare test successful! Your config is ready to use.");
                    callback(true, null);
                }
                else
                {
                    MelonLogger.Error($"Cloudflare test failed: {error}");
                    callback(false, error);
                }
            });
        }

        private static string CreateRequestJson(string systemPrompt,
                                                string userMessage)
        {
            return $"{{\"prompt\":\"{JsonUtil.EscapeJsonString(systemPrompt)}\\n{JsonUtil.EscapeJsonString(userMessage)}\"}}";
        }

        private string ParseCloudflareResponse(string jsonResponse)
        {
            try
            {
                // Match "result.response"
                var match = System.Text.RegularExpressions.Regex.Match(
                    jsonResponse, "\"response\"\\s*:\\s*\"(.*?)\"",
                    System.Text.RegularExpressions.RegexOptions.Singleline);

                if (!match.Success)
                {
                    return "Error: Unable to find 'response' in the Cloudflare response.";
                }

                // Extract and unescape the content
                string response = match.Groups[1].Value;
                response = JsonUtil.UnescapeJsonString(response);

                return response;
            }
            catch (System.Exception)
            {
                return "Error: Unable to parse Cloudflare response.";
            }
        }

        [System.Serializable]
        public class CloudflareResponse
        {
            public Result result;
            public bool success;
        }

        [System.Serializable]
        public class Result
        {
            public string response;
        }
    }
}
