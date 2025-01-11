# matechat
[ c# ] MelonLoader mod for Desktop Mate that allows you to chat with your current mate using LLM APIs

This currently relies on having a cloudflare worker running meta/llama-3-8b-instruct. The next steps in development are implmenting more choices of LLM APIs, and key/settings management.

# Usage
- Install the latest [MelonLoader](https://github.com/LavaGang/MelonLoader/) to Desktop Mate
- Run Desktop Mate once and then exit
- Replace XXX and YYY (lines 33/34) with correct values for LLM API
- Build solution and put matechat.dll into Desktop Mate mods folder
- Run Desktop Mate and use F8 to open chat window
