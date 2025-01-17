using MelonLoader;

using UnityEngine;

using matechat.util;

namespace matechat
{
    public static class Config
    {
        public static MelonPreferences_Category category;

        public static MelonPreferences_Entry<KeyCode> CHAT_KEYBIND;
        public static MelonPreferences_Entry<string> AI_NAME;
        public static MelonPreferences_Entry<string> API_KEY;
        public static MelonPreferences_Entry<string> API_URL;
        public static MelonPreferences_Entry<string> SYSTEM_PROMPT;

        public static MelonPreferences_Entry<int> CHAT_WINDOW_WIDTH;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_HEIGHT;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_X;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_Y;
        public static MelonPreferences_Entry<int> CHAT_WINDOW_FONT_SIZE;

        public static string lastUsedSystemPrompt;

        public static void Initialize()
        {
            category = MelonPreferences.CreateCategory("MateChat");

            CHAT_KEYBIND = category.CreateEntry("CHAT_KEYBIND", KeyCode.F8);
            AI_NAME = category.CreateEntry("AI_NAME", "Desktop Mate");
            API_KEY = category.CreateEntry("API_KEY", "xxx");
            API_URL = category.CreateEntry("API_URL", "https://api.cloudflare.com/client/v4/accounts/XXX/ai/run/@cf/meta/llama-3-8b-instruct");
            SYSTEM_PROMPT = category.CreateEntry("SYSTEM_PROMPT", "You are a cheerful digital companion inspired by Hatsune Miku! Keep responses brief and energetic. Use musical notes (♪), kaomoji (◕‿◕), and cute text markers (✧) naturally. Express yourself in a sweet, J-pop idol style while being helpful and concise. Add '~' to soften statements occasionally. End responses with a musical note or kaomoji when fitting. Keep answers short and direct, but always maintain a cute and supportive tone!");

            CHAT_WINDOW_WIDTH = category.CreateEntry("CHAT_WINDOW_WIDTH", 400);
            CHAT_WINDOW_HEIGHT = category.CreateEntry("CHAT_WINDOW_HEIGHT", 500);
            CHAT_WINDOW_X = category.CreateEntry("CHAT_WINDOW_X", 20);
            CHAT_WINDOW_Y = category.CreateEntry("CHAT_WINDOW_Y", 20);
            CHAT_WINDOW_FONT_SIZE = category.CreateEntry("CHAT_WINDOW_FONT_SIZE", 16);

            category.SetFilePath("UserData/MateChat.cfg");
            category.SaveToFile();

            lastUsedSystemPrompt = SYSTEM_PROMPT.Value;
        }

        public static bool TestConfig()
        {
            bool isValid = true;

            void LogError(string message)
            {
                MelonLogger.Error(message);
                isValid = false;
            }

            // API Key
            if (string.IsNullOrEmpty(API_KEY.Value) || API_KEY.Value == "xxx")
                LogError("API Key is not configured!");
            else if (API_KEY.Value.Length < 32)
                LogError("API Key appears to be too short - please verify your key!");

            // API URL
            if (string.IsNullOrEmpty(API_URL.Value))
                LogError("API URL is empty!");
            else if (API_URL.Value.Contains("XXX"))
                LogError("API URL contains placeholder 'XXX' - please update with your account ID!");
            else if (!Uri.TryCreate(API_URL.Value, UriKind.Absolute, out Uri uriResult) ||
                     (!uriResult.Scheme.Equals("http") && !uriResult.Scheme.Equals("https")))
                LogError("API URL is not a valid HTTP/HTTPS URL!");
            else if (API_URL.Value.Contains("cloudflare.com"))
            {
                if (!API_URL.Value.Contains("/ai/run/"))
                    LogError("Cloudflare AI URL appears to be invalid - should contain '/ai/run/'!");
                if (!API_URL.Value.Contains("/accounts/"))
                    LogError("Cloudflare AI URL appears to be invalid - should contain '/accounts/'!");
            }

            // System Prompt
            if (string.IsNullOrEmpty(SYSTEM_PROMPT.Value))
                LogError("System prompt is empty!");
            else if (SYSTEM_PROMPT.Value.Length > 4096)
                LogError("System prompt exceeds recommended length (4096 characters)!");

            // Window Configuration
            ValidateRange(CHAT_WINDOW_WIDTH.Value, 200, Screen.width, "chat window width");
            ValidateRange(CHAT_WINDOW_HEIGHT.Value, 200, Screen.height, "chat window height");
            ValidateRange(CHAT_WINDOW_FONT_SIZE.Value, 8, 72, "font size");

            // AI Name
            if (string.IsNullOrEmpty(AI_NAME.Value))
                LogError("AI Name is not configured!");
            else if (AI_NAME.Value.Length > 32)
                LogError("AI Name is too long (maximum 32 characters)!");

            // Window Position
            ValidateRange(CHAT_WINDOW_X.Value, 0, Screen.width - 100, "window X position");
            ValidateRange(CHAT_WINDOW_Y.Value, 0, Screen.height - 100, "window Y position");

            return isValid;

            void ValidateRange(int value, int min, int max, string name)
            {
                if (value < min || value > max)
                    LogError($"Invalid {name}: {value}. Should be between {min} and {max}!");
            }
        }


        public static void ReloadConfig()
        {
            category.LoadFromFile();
            if (TestConfig())
            {
                if (lastUsedSystemPrompt != SYSTEM_PROMPT.Value)
                {
                    MelonLogger.Msg("System prompt changed, sending reset request...");
                    MelonCoroutines.Start(CloudflareUtil.SendCloudflareRequest("Acknowledge the system prompt change.", SYSTEM_PROMPT.Value));
                    lastUsedSystemPrompt = SYSTEM_PROMPT.Value;
                }

                Core.GetChatFeature()?.UpdateSettings();
                MelonLogger.Msg("Config reloaded successfully!");
                MelonCoroutines.Start(CloudflareUtil.TestCloudflareWorker());
            }
            else
            {
                MelonLogger.Error("Config reload failed - please check your settings!");
            }
        }
    }
}
