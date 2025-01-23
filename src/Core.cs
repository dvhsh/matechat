using MelonLoader;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using matechat.sdk.Feature;
using matechat.feature;
using matechat.ui;
using matechat.util;
using static matechat.database.DatabaseManager;
using matechat.database;

namespace matechat
{
    public class Core : MelonMod
    {
        private static List<Feature> features;
        private MenuManager menuManager;
        private bool isInitialized;

        public static DatabaseManager databaseManager;

        private static AIEngineManager aiEngineManager;

        public override void OnApplicationStart()
        {
            try
            {
                // Initialize the configuration
                Config.Initialize();

                // Initialize AI engines
                InitializeAIEngines();


                //Initialize the Database Engine
                InitializeDBEngine();

                // Initialize features
                features = new List<Feature>
                {
                    new ChatFeature()
                };

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

        private void InitializeDBEngine()
        {
            databaseManager = new DatabaseManager("UserData\\ChatLog.db");
        }

        private static void InitializeAIEngines()
        {
            try
            {
                var engines = new List<(string Name, IAIEngine Engine)>();
                string engineType = Config.ENGINE_TYPE.Value?.ToLower();

                switch (engineType)
                {
                    case "cloudflare":
                        engines.Add((
                            "Cloudflare",
                            new CloudflareEngine(
                                Config.API_KEY.Value,
                                Config.ACCOUNT_ID.Value,
                                Config.MODEL_NAME.Value
                            )
                        ));
                        MelonLogger.Msg("Added Cloudflare engine.");
                        break;

                    case "openai":
                        engines.Add((
                            "OpenAI",
                            new OpenAIEngine(
                                Config.API_KEY.Value,
                                "https://api.openai.com/v1/chat/completions"
                            )
                        ));
                        MelonLogger.Msg("Added OpenAI engine.");
                        break;

                    case "openrouter":
                        engines.Add((
                            "OpenRouter",
                            new OpenAIEngine(
                                Config.API_KEY.Value,
                                "https://openrouter.ai/api/v1/chat/completions"
                            )
                        ));
                        MelonLogger.Msg("Added OpenRouter engine.");
                        break;

                    default:
                        throw new System.Exception($"Unsupported AI engine type: {engineType}");
                }

                if (engines.Count == 0)
                {
                    throw new System.Exception("No valid AI engine configurations found.");
                }

                aiEngineManager = new AIEngineManager(engines[0].Name, engines.ToArray());

                // Debugging: Log registered engines
                foreach (var engine in engines)
                {
                    MelonLogger.Msg($"Registered engine: {engine.Name}");
                }

                MelonLogger.Msg("AIEngineManager initialized successfully.");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to initialize AI engines: {ex.Message}");
                throw;
            }
        }

        public static void ReloadAIEngines()
        {
            try
            {
                InitializeAIEngines();
                MelonLogger.Msg("AI engines reloaded successfully.");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to reload AI engines: {ex.Message}");
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

        public static AIEngineManager GetAIEngineManager()
        {
            if (aiEngineManager == null)
            {
                throw new System.Exception("AIEngineManager is not initialized.");
            }
            return aiEngineManager;
        }

        public override void OnLateUpdate()
        {
            if (!isInitialized || features == null)
                return;

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
            if (!isInitialized || features == null)
                return;

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
