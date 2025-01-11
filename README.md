# matechat
[ c# ] MelonLoader mod for Desktop Mate that allows you to chat with your current mate using LLM APIs

This currently relies on having a cloudflare worker running meta/llama-3-8b-instruct. The next steps in development are implmenting more choices of LLM APIs, and key/settings management.

# Usage
- Install the latest [MelonLoader](https://github.com/LavaGang/MelonLoader/) to Desktop Mate
- Run Desktop Mate once and then exit
- Replace XXX and YYY (lines 33/34) with correct values for LLM API
- Build solution and put matechat.dll into Desktop Mate mods folder
- Run Desktop Mate and use F8 to open chat window

# Planned Features
- Integrate settings selection into Desktop Mate's settings menu
- Allow user to pick from multiple LLM APIs
- Allow user to input and store API keys via settings menu
- Allow user to modify system prompt via settings menu
- Allow user to rebind the chat bot toggle via settings menu
- Allow user to move the chat box
- Allow user to scroll in the chat box and input box
- Proper JSON deserialization
- Reworked error handling
- Make the chat box nicer visually, maybe do some type of speech bubble response thing
