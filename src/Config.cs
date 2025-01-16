using MelonLoader;

using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Text;
using matechat.feature;

namespace matechat
{
    public static class Config
    {
        public static MelonPreferences_Category category;

        public static MelonPreferences_Entry<KeyCode> CHAT_KEYBIND;
        public static MelonPreferences_Entry<string> API_KEY;
        public static MelonPreferences_Entry<string> API_URL;
        public static MelonPreferences_Entry<string> SYSTEM_PROMPT;

        public static MelonPreferences_Entry<int> CHAT_WINDOW_WIDTH;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_HEIGHT;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_X;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_Y;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_FONT_SIZE;
        public static MelonPreferences_Entry<string> AI_NAME;

        public static void Initialize()
        {
            category = MelonPreferences.CreateCategory("MateChat");

            CHAT_KEYBIND = category.CreateEntry("CHAT_KEYBIND", KeyCode.F8);
            API_KEY = category.CreateEntry("API_KEY", "xxx");
            API_URL = category.CreateEntry("API_URL", "https://api.cloudflare.com/client/v4/accounts/XXX/ai/run/@cf/meta/llama-3-8b-instruct");
            SYSTEM_PROMPT = category.CreateEntry("SYSTEM_PROMPT", "You are a cheerful digital companion inspired by Hatsune Miku! Keep responses brief and energetic. Use musical notes (♪), kaomoji (◕‿◕), and cute text markers (✧) naturally. Express yourself in a sweet, J-pop idol style while being helpful and concise. Add '~' to soften statements occasionally. End responses with a musical note or kaomoji when fitting. Keep answers short and direct, but always maintain a cute and supportive tone!");

            CHAT_WINDOW_WIDTH = category.CreateEntry("CHAT_WINDOW_WIDTH", 400);
            CHAT_WINDOW_HEIGHT = category.CreateEntry("CHAT_WINDOW_HEIGHT", 500);
            CHAT_WINDOW_X = category.CreateEntry("CHAT_WINDOW_X", 20);
            CHAT_WINDOW_Y = category.CreateEntry("CHAT_WINDOW_Y", 20);
            CHAT_WINDOW_FONT_SIZE = category.CreateEntry("CHAT_WINDOW_FONT_SIZE", 16);
            AI_NAME = category.CreateEntry("AI_NAME", "Mate");

            category.SetFilePath("UserData/MateChat.cfg");
        }

        public static bool TestConfig()
        {
            if (string.IsNullOrEmpty(API_KEY.Value) || API_KEY.Value == "xxx")
            {
                MelonLogger.Error("API Key is not configured!");
                return false;
            }

            if (string.IsNullOrEmpty(API_URL.Value) || API_URL.Value.Contains("XXX"))
            {
                MelonLogger.Error("API URL is not configured!");
                return false;
            }

            return true;
        }

        public static IEnumerator TestCloudflareWorker()
        {
            if (!TestConfig()) yield break;

            string testMessage = "{\"messages\":[{\"role\":\"system\",\"content\":\"Test\"},{\"role\":\"user\",\"content\":\"test\"}]}";

            UnityWebRequest webRequest = new UnityWebRequest(API_URL.Value, "POST");
            try
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(testMessage);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", $"Bearer {API_KEY.Value}");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    MelonLogger.Msg("Cloudflare Worker test successful! Your config is ready to use!");
                }
                else
                {
                    MelonLogger.Error($"Cloudflare Worker test failed: {webRequest.error}");
                    switch (webRequest.responseCode)
                    {
                        case 401:
                        case 403:
                            MelonLogger.Error("Authentication failed - please check your API key!");
                            break;
                        case 404:
                            MelonLogger.Error("API URL not found - please check your account ID and endpoint!");
                            break;
                    }
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

        public static void ReloadConfig()
        {
            category.LoadFromFile();
            if (TestConfig())
            {
                Core.GetChatFeature()?.UpdateSettings();
                MelonLogger.Msg("Config reloaded successfully!");
                MelonCoroutines.Start(TestCloudflareWorker());
            }
            else
            {
                MelonLogger.Error("Config reload failed - please check your settings!");
            }
        }
    }
}
