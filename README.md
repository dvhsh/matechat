# matechat
[ c# ] MelonLoader mod for Desktop Mate that allows you to chat with your current mate using LLM APIs

This currently relies on having a cloudflare worker running meta/llama-3-8b-instruct. The next steps in development are implmenting more choices of LLM APIs.

You can easily setup a CloudFlare AI Worker and obtain your API_URL and API_KEY by following this [short guide](https://developers.cloudflare.com/workers-ai/get-started/rest-api/).

# Usage
- Install the latest [MelonLoader](https://github.com/LavaGang/MelonLoader/) to Desktop Mate
- Run Desktop Mate once and then exit
- Put latest matechat.dll into your Mods folder
- Run Desktop Mate once and then exit
- Edit UserData/MateChat.cfg to include your credentials and preferences
- Run Desktop Mate and use assigned key to open the chat menu

Please note that if you are to edit MateChat.cfg directly, you'll need to restart Desktop Mate for changes to take effect.

# Planned Features
- Allow user to pick from multiple LLM APIs
- Allow user to input and store API keys via settings menu
- Allow user to modify system prompt via settings menu
- Allow user to rebind the chat bot toggle via settings menu
- Allow user to move the chat box
- Allow user to scroll in the chat box and input box
- Proper JSON deserialization
- Reworked error handling
- Make the chat box nicer visually, maybe do some type of speech bubble response thing

# In Progress
- Integrate settings selection into Desktop Mate's settings menu
- Adding menu transition and close animations (matechat currently can add a button to the menu, and clone the back/close buttons onto its own page)
