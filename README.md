<img src="matechat.png" alt="MateChat Banner" width="100%">

# MateChat
> A MelonLoader mod that enables AI chat and voice interactions with your Desktop Mate! ♪

MateChat now supports multiple AI language models and text-to-speech capabilities, allowing for even more immersive interactions with your desktop companion~ ✧

## ✨ Features
- Chat with your Desktop Mate using various AI providers
- Text-to-Speech support powered by GPT-SoVITS
- Cute and minimal interface matching the game's aesthetic
- Highly configurable settings for both chat and voice
- Quick access through mate's menu or keyboard shortcut

## 🎀 Installation
**More detailed instructions (step by step) can be found in the [Wiki](https://github.com/dvhsh/matechat/wiki).

1. **Install Desktop Mate & MelonLoader**
   - Download the latest `Desktop.Mate.zip`
   - Extract the contents to match your Steam installation directory
   - The extracted folder should contain:
     - `Mods/` directory with the MateChat dll
     - `UserLibs/` with required libraries

2. **Configure MateChat**
   - Start Desktop Mate
   - Right-click and select MateChat from the menu
   - Configure your settings in `UserData/MateChat.cfg`

3. **Setup Text-to-Speech (Optional)**
   - Clone [GPT-SoVITS repository](https://github.com/RVC-Boss/GPT-SoVITS)
   - Install [pretrained models](https://github.com/RVC-Boss/GPT-SoVITS?tab=readme-ov-file#pretrained-models)
   - Run `pip install -r requirements.txt`
   - Place a reference audio clip (<10s) in the root directory as `reference.wav`
   - Start the TTS server: `python api_v2.py`

## 📝 Configuration
```ini
[MateChat]
# Core Settings
CHAT_KEYBIND = "F8"
ENGINE_TYPE = "Cloudflare"
API_KEY = "your_api_key"
ACCOUNT_ID = "your_account_id"  # Optional for OpenRouter/OpenAI
MODEL_NAME = "llama-3.1-8b-instruct"
NAME = "USER"
AI_NAME = "Miku"

# System Prompt
SYSTEM_PROMPT = "You are a cheerful digital companion inspired by Hatsune Miku! Keep responses brief and energetic..."

# UI Settings
CHAT_WINDOW_WIDTH = 400
CHAT_WINDOW_HEIGHT = 500
CHAT_WINDOW_X = 20
CHAT_WINDOW_Y = 20
CHAT_WINDOW_FONT_SIZE = 24

# Advanced Features
ENABLE_AUDIO_MODEL = false
BASE_URL = ""
```

## 🎤 Text-to-Speech Configuration

Enable and configure TTS in your `MateChat.cfg`:

```ini
# Text-to-Speech Settings
ENABLE_TTS = true
ENABLE_AUDIO_MODEL = false      # Not yet supported, placeholder

TTS_ENGINE = "GPT-SoVITS"      # Currently only GPT-SoVITS is tested
TTS_API_URL = "http://localhost:9880"  # Default GPT-SoVITS server URL

# Voice Configuration
TTS_TEXT_LANG = "auto"         # Language detection for input text
TTS_REF_AUDIO_PATH = "reference.wav"   # Reference voice file (on server side)
TTS_PROMPT_TEXT = "[voice of reference.wav]"
TTS_PROMPT_LANG = "ja"         # Reference language (ja/zh/en/etc)

# Processing Settings
TTS_TEXT_SPLIT_METHOD = "cut5"
TTS_BATCH_SIZE = 1
TTS_MEDIA_TYPE = "wav"
TTS_STREAMING_MODE = false      # Streaming support pending GPT-SoVITS updates
```

### Important Notes:
- The `reference.wav` path is relative to your GPT-SoVITS server directory
- Use a clear, high-quality voice sample under 10 seconds for best results
- Keep the GPT-SoVITS server running while using TTS features
- Streaming mode is implemented but waiting on full GPT-SoVITS support

## 💭 Coming Soon
- UI overhaul with improved aesthetics
- Speech bubble integration
- Enhanced character animations
- Speech-to-text capabilities
- And more exciting features! ♪

## 🌟 Support
Join our [Discord server][discord-url] for help, updates, and cute chat screenshots!

The mod performs automatic configuration testing on startup and reload to ensure everything is working correctly. Check the MelonLoader console for detailed feedback~

---
Made with ♥ for Desktop Mate

[discord-url]: https://discord.gg/Xu7pEU24kw
