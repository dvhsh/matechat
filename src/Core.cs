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
        private static IAIEngine aiEngine;

        private static AIEngineManager aiEngineManager;
        private static IAudioProcessor audioEngine;

        public override void OnApplicationStart()
        {
            try
            {
                // Initialize the configuration
                Config.Initialize();

                // Initialize AI engines
                InitializeAIEngines();

                // initialize the Audio engine based on the config
                InitializeAudioEngine();



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

                    case "OpenAICompatible":
                        aiEngine = new OpenAIUtil();
                        MelonLogger.Msg("Using OpenAI(OpenAICompatible) engine.");
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

        private static void InitializeAudioEngine()
        {
            try
            {
                if (Config.ENABLE_TTS.Value)
                {
                    switch (Config.TTS_ENGINE.Value)
                    {
                        case "GPT-SoVITS":
                            audioEngine = new TTSEngine();
                            MelonLogger.Msg("Using GPT-SoVITS for TTS.");
                            break;

                        default:
                            MelonLogger.Error($"Unsupported TTS engine type: {Config.TTS_ENGINE.Value}");
                            throw new System.Exception("Invalid TTS engine configuration.");
                    }
                }
                else if (Config.ENABLE_AUDIO_MODEL.Value)
                {
                    audioEngine = new AudioModelEngine();
                    MelonLogger.Msg("Using Audio Model for sound processing.");
                }
                else
                {
                    MelonLogger.Msg("No Audio Engine enabled.");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to initialize Audio Engine: {ex.Message}");
                throw;
            }
        }
        public static void ReloadAudioEngine()
        {
            try
            {
                InitializeAudioEngine();
                MelonLogger.Msg("Audio engine reloaded successfully.");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to reload Audio engine: {ex.Message}");
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

        public static IAudioProcessor GetAudioEngine()
        {
            if (audioEngine == null)
            {
                throw new System.Exception("Audio engine is not initialized.");
            }
            return audioEngine;
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
