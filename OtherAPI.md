# Configuring for other APIs

This page will detail what chanegs need to be made to connect MateChat to other API providers.

This assumes that you have installed MelonLoader and MateChat, and that they are already running correctly.

## CloudFlare (Default model)
1. Create an account at [CloudFlare](https://dash.cloudflare.com/sign-up)
2. Once you are logged into the CloudFlare dashboard:
    - Click on AI on the left hand menu
        - The click on Workers API.
    - Click on 'Use REST API' at the topof the screen.
    - Click Create a Workers AI API Token
        - This will pop a window open on the right. You can change the name of the token here.
        - Click Create API Token at the bottom.
        - Copy the API token that is created. This is the only time you will see the token.
        - Click Finish at the bottom.
    - The token that you just created will be added to the config file as the API_KEY.
    - Copy the Account ID. This will be added to the config file as the ACCOUNT_ID.
3. Click Back at the top of the screen. 
    - Select and copy the Model that you want to use. You will only need the part ater the slash
        - For example, CloudFlare's @cf/meta/llama-3-8b-instruct model, you would just need the llama-3-8b-instruct portion.
    - This will be added to the config file as MODEL_NAME.

Need help setting up your Cloudflare AI Worker? Follow this [quick guide](https://developers.cloudflare.com/workers-ai/get-started/rest-api/)!

## OpenRouter
1. Create an account at [OpenRouter](https://openrouter.ai/)
    - Click Sign In at the upper left.
    - Click Sign Up at the bottom of the window that pops up.
2. Once you are logged into the OpenRouter dashboard:
    - Click on your account in the upper right corner and select Keys.
    - Click Create Key.
        - Give the key a name and click Create.
    - Copy the key that is created. This is the only time you will see the key.
    - The key that you just created will be added to the config file as the API_KEY.


## OpenAI
1. Create an account at [OpenAI](https://platform.openai.com/docs/overview)
    - Click on Sign up in the upper right corner.
2. Once you are logged into the OpenAI dashboard:
    - Click Start Building at the top of the screen.
        - Enter an Organization Name (can be anything).
        - Click Create Team.
        - Click "I'll invite my team later".
    - Give your key a name.
    - Click Generate API Key.
    - Copy the API Key that is generated.
    - The key that you just created will be added to the config file as the API_KEY.
        - You do not need an Account ID.
    - Click Continue.
    - Click "I'll buy credits later"
3. You should be back at the OpenAI Developer Platform page.
    - Select a Model, such as gpt-4o-mini, gpt-3.5-turbo or gpt-4.
    - This will be added to the config file as MODEL_NAME.


## Once you have your API_KEY and ACCOUNT_ID
**Configure the Mod**
   - Start Desktop Mate
   - Right-click and select MateChat from the menu
   - Click "Open Config" and add your credentials:
     - `API_KEY` (. your API token)
     - `ACCOUNT_ID` (your account id. This is not needed for OpenAI )
   - Click "Reload Config"


## Example Config
```
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
