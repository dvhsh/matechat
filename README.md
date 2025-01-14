# matechat
[ c# ] MelonLoader mod for Desktop Mate that allows you to chat with your current mate using LLM APIs

This currently relies on having a cloudflare worker running meta/llama-3-8b-instruct. 

You can easily setup a CloudFlare AI Worker and obtain your `API_URL` and `API_KEY`(token) by following this [short guide](https://developers.cloudflare.com/workers-ai/get-started/rest-api/).

You can test your CloudFlare AI Worker by running this command in Command Prompt (replace `ACCOUNT_ID` and `API_KEY`)
```
curl -X POST https://api.cloudflare.com/client/v4/accounts/ACCOUNT_ID/ai/run/@cf/meta/llama-3-8b-instruct -H "Authorization: Bearer API_KEY" -d "{\"messages\":[{\"role\":\"system\",\"content\":\"You are a friendly assistant\"},{\"role\":\"user\",\"content\":\"hi \"}]}"
```

[Desktop Mate Modding discord server](https://discord.gg/Xu7pEU24kw)

# Usage
- Install the latest [MelonLoader](https://github.com/LavaGang/MelonLoader/) to Desktop Mate
- Run Desktop Mate once and then exit
- Put latest `matechat.dll` into your Mods folder
- Run Desktop Mate and right click to open it's menu, select MateChat
- Click the Open Config button, edit the configuration to include your credentials(`API_KEY`, `API_URL`), save, then press Reload Config
- You are now ready to chat with your Desktop Mate!

# Planned Features
- Allow user to pick from multiple LLM APIs
- Make the chat box nicer visually, maybe do some type of speech bubble response thing
- Adding Desktop Mate animations to the mods menu
