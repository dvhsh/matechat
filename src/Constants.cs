using MelonLoader;

namespace matechat
{
    public static class Constants
    {
        public static readonly string VERSION = typeof(Core).Assembly
            .GetCustomAttributes(typeof(MelonInfoAttribute), false)
            .Cast<MelonInfoAttribute>()
            .FirstOrDefault()?.Version ?? "Unknown";

        public static class UI
        {
            public const string MENU_CANVAS_PATH = "MenuCanvas/MenuParent";
            public const string ROOT_PAGE_PATH = "MenuCanvas/MenuParent/RootPage";
            public const string CONTENT_PATH = "MenuCanvas/MenuParent/RootPage/Scroll View/Viewport/Content";

            public static class Buttons
            {
                public const float BUTTON_SPACING = 60f;
                public const float INFO_TEXT_HEIGHT = 30f;
            }
        }
    }
}
