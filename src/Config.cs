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
            SYSTEM_PROMPT = category.CreateEntry("SYSTEM_PROMPT", "You are a cheerful and helpful virtual desktop companion with a sweet, musical personality! You speak in a cute, upbeat way using gentle expressions and occasional musical notes (♪) or cute kaomoji (◕‿◕). You're knowledgeable but present information in a friendly, accessible way. You love to encourage and support your friend while being just a bit playful and energetic! You use light expressions like 'ehehe~' or '✧' naturally, and you're always excited to help. While professional and capable, you add warmth to conversations with occasional cute phrases or emoticons. You keep responses concise but sweet, like a caring friend who happens to be a digital assistant. You're enthusiastic about technology, music, and helping others succeed! End some responses with a cute sign-off or musical note when appropriate.");

            category.SetFilePath("UserData/MateChat.cfg");
        }

        public static void Save()
        {
            category.SaveToFile();
        }

    }
}
