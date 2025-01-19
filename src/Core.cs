using MelonLoader;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using matechat.sdk.Feature;
using matechat.feature;
using matechat.ui;
using matechat.util;
using matechat.util.matechat.util;

namespace matechat
{
    public class Core : MelonMod
    {
        private static List<Feature> features;
        private MenuManager menuManager;
        private bool isInitialized;

        // Field to store the selected AI engine
        private static IAIEngine aiEngine;

        public override void OnApplicationStart()
        {
            try
            {
                // Initialize the configuration
                Config.Initialize();

                // Initialize the AI engine based on the config
                InitializeAIEngine();

                // Initialize features
                features = new List<Feature>();
                features.Add(new ChatFeature());

                menuManager = new MenuManager();
                isInitialized = true;

                // Reload configuration
                Config.ReloadConfig();
                LoggerInstance.Msg("Initialized.");

                // Wait a second before starting mod (@FIX)
                MelonCoroutines.Start(DelayedMenuInit());
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error($"Failed to initialize: {ex}");
            }
        }

        private void InitializeAIEngine()
        {
            switch (Config.ENGINE_TYPE.Value) // Add ENGINE_TYPE to Config
            {
                //Switch Case Based On engine
                case "Cloudflare":
                    aiEngine = new CloudflareUtil(); // Or CloudflareEngine
                    LoggerInstance.Msg("Using Cloudflare engine.");
                    break;
                case "OpenRouter":
                    aiEngine = new OpenRouterEngine(); 
                    LoggerInstance.Msg("Using OpenRouter engine.");
                    break;
                case "OpenAI":
                    aiEngine = new OpenAIUtil();
                    LoggerInstance.Msg("Using OpenAI engine.");
                    break;
                default:
                    LoggerInstance.Error($"Unsupported AI engine type: {Config.ENGINE_TYPE.Value}");
                    throw new System.Exception("Invalid AI engine configuration.");
            }
        }

        private IEnumerator DelayedMenuInit()
        {
            yield return new WaitForSeconds(1f);
            MelonCoroutines.Start(menuManager.WaitForMenu());
        }

        public static ChatFeature GetChatFeature()
        {
            return features?.FirstOrDefault(f => f is ChatFeature) as ChatFeature;
        }

        // Method to retrieve the selected AI engine
        public static IAIEngine GetAIEngine()
        {
            if (aiEngine == null)
            {
                throw new System.Exception("AI engine is not initialized.");
            }
            return aiEngine;
        }

        public override void OnLateUpdate()
        {
            if (!isInitialized || features == null) return;

            foreach (var feature in features)
            {
                try
                {
                    if (Input.GetKeyDown(feature.Keybind))
                    {
                        feature.Toggle();
                    }
                }
                catch (System.Exception ex)
                {
                    LoggerInstance.Error($"Error in feature update: {ex}");
                }
            }
        }

        public override void OnGUI()
        {
            if (!isInitialized || features == null) return;

            foreach (var feature in features)
            {
                try
                {
                    if (feature.IsEnabled && feature is ChatFeature chatFeature)
                    {
                        chatFeature.DrawGUI();
                    }
                }
                catch (System.Exception ex)
                {
                    LoggerInstance.Error($"Error in GUI update: {ex}");
                }
            }
        }
    }
}
