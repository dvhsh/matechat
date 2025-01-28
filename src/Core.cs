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
        public static DatabaseAudioManager databaseAudioManager;

        private static AIEngineManager aiEngineManager;
        private static AudioEngineManager audioEngineManager;

        public override void OnApplicationStart()
        {
            try
            {
                // Initialize the configuration
                Config.Initialize();

                // Initialize AI engines
                InitializeAIEngines();

                // initialize the Audio engine based on the config
                InitializeAudioEngines();

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
            databaseAudioManager = new DatabaseAudioManager("UserData\\AudioLog.db");
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

                    case "openaicompatible":
                        engines.Add((
                            "OpenAICompatible",
                            new OpenAIEngine(
                                Config.API_KEY.Value,
                                $"{Config.BASE_URL.Value}/v1/chat/completions"
                            )
                        ));
                        MelonLogger.Msg("Added OpenAICompatible engine.");
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

        private static void InitializeAudioEngines()
        {
            var engines = new List<(string Name, IAudioEngine ttsEngine)>();

            try
            {
                if (Config.ENABLE_TTS.Value)
                {

                    string endpoint = null;
                    switch (Config.TTS_ENGINE.Value)
                    {
                        case "GPT-SoVITS":
                            endpoint = $"{Config.TTS_API_URL.Value}/tts";
                            break;
                        // TODO: add other engine features
                        default:
                            throw new System.Exception($"Unsupported TTS engine: {Config.TTS_ENGINE.Value}");
                    }

                    // 이제 스트리밍 모드인지 아닌지에 따라 등록
                    if (Config.TTS_STREAMING_MODE.Value)
                    {
                        // 스트리밍 엔진
                        engines.Add((
                            $"{Config.TTS_ENGINE.Value} (Streaming)", // 엔진 이름
                            new StreamingTTSEngine(Config.TTS_ENGINE.Value, endpoint)
                        ));
                        MelonLogger.Msg($"[AudioEngine] Added streaming TTS engine: {Config.TTS_ENGINE.Value}");
                    }
                    else
                    {
                        // 일반 TTS
                        engines.Add((
                            Config.TTS_ENGINE.Value, // 엔진 이름
                            new TTSEngine(Config.TTS_ENGINE.Value, endpoint)
                        ));
                        MelonLogger.Msg($"[AudioEngine] Added non-streaming TTS engine: {Config.TTS_ENGINE.Value}");
                    }
                }
                else
                {
                    MelonLogger.Msg("[AudioEngine] TTS is disabled in config.");
                }

                if (engines.Count == 0)
                {
                    audioEngineManager = null;
                    MelonLogger.Warning("No TTS engines were initialized. AudioEngineManager is set to null.");
                    return;
                }

                audioEngineManager = new AudioEngineManager(
                    defaultEngine: engines[0].Name,
                    engines: engines.ToArray()
                );
                MelonLogger.Msg($"[AudioEngine] AudioEngineManager initialized with default engine: {engines[0].Name}");
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
                InitializeAudioEngines();
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

        public static AudioEngineManager GetAudioEngine()
        {
            if (audioEngineManager == null)
            {
                InitializeAudioEngines();
               // throw new System.Exception("Audio engine is not initialized.");
            }
            return audioEngineManager;
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
