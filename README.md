<img src="matechat.png" alt="MateChat Banner" width="100%">

# MateChat
> A MelonLoader mod that lets you chat with your Desktop Mate using AI! ♪

MateChat adds a cute chat interface to Desktop Mate, powered by Cloudflare's AI Workers running the Llama 2 language model. Have natural conversations with your mate in an adorable interface designed to match Desktop Mate's aesthetic~ ✧

## ✨ Features
- Chat with your Desktop Mate using AI
- Cute and minimal interface that matches the game's style
- Easy to setup and configure
- Customizable chat window and font size
- Quick access through the mate's menu or keyboard shortcut

## 🎀 Installation

1. **Setup MelonLoader**
   - Install the latest [MelonLoader](https://github.com/LavaGang/MelonLoader/)
   - Run Desktop Mate once and exit

2. **Install MateChat**
   - Download the latest `matechat.dll` from [Releases](https://github.com/dvhsh/matechat/releases)
   - Place it in your Desktop Mate's Mods folder

3. **Configure the Mod**
   - Start Desktop Mate
   - Right-click and select MateChat from the menu
   - Click "Open Config" and add your Cloudflare credentials:
     - `API_KEY` (your API token)
     - `API_URL` (your worker URL)
   - Click "Reload Config"

Need help setting up your Cloudflare AI Worker? Follow this [quick guide](https://developers.cloudflare.com/workers-ai/get-started/rest-api/)!

## Example Config
```
[MateChat]
[MateChat]
CHAT_KEYBIND = "F8"
ENGINE_TYPE = "Cloudflare"
ACCOUNT_ID = ""
API_KEY = ""
MODEL_NAME = "llama-3-8b-instruct"
SYSTEM_PROMPT = "You are a cheerful digital companion inspired by Hatsune Miku!"
AI_NAME = "Desktop Mate"
CHAT_WINDOW_WIDTH = 400
CHAT_WINDOW_HEIGHT = 500
CHAT_WINDOW_X = 20
CHAT_WINDOW_Y = 20
CHAT_WINDOW_FONT_SIZE = 16
```

## 💭 Coming Soon
- Support for multiple LLM APIs
- Visual improvements including speech bubbles
- Desktop Mate animations integration
- And more! ♪

## 🌟 Support
Join our [Discord server][discord-url] for help, updates, and cute chat screenshots!

The mod automatically tests your config on startup and reloads, checking if your worker is setup correctly. Check the MelonLoader console for detailed feedback~

---
Made with ♥ for Desktop Mate

[discord-url]: https://discord.gg/Xu7pEU24kw
