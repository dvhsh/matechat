using MelonLoader;
using UnityEngine;

namespace matechat
{
    public static class Config
    {
        private static MelonPreferences_Category category;
        public static MelonPreferences_Entry<KeyCode> CHAT_KEYBIND;
        public static MelonPreferences_Entry<string> API_KEY;
        public static MelonPreferences_Entry<string> API_URL;
        public static MelonPreferences_Entry<string> SYSTEM_PROMPT;

        public static void Initialize()
        {
            category = MelonPreferences.CreateCategory("MateChat");

            CHAT_KEYBIND = category.CreateEntry("CHAT_KEYBIND", KeyCode.F8);
            API_KEY = category.CreateEntry("API_KEY", "xxx");
            API_URL = category.CreateEntry("API_URL", "https://api.cloudflare.com/client/v4/accounts/XXX/ai/run/@cf/meta/llama-3-8b-instruct");
            SYSTEM_PROMPT = category.CreateEntry("SYSTEM_PROMPT", "You are a cute and helpful desktop mate assistant. Keep your responses concise and friendly.");

            category.SetFilePath("UserData/MateChat.cfg");
        }

        public static void Save()
        {
            category.SaveToFile();
        }

    }
}
