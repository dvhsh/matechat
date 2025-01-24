using MelonLoader;
using UnityEngine;

using System.Collections;

using matechat.sdk.Feature;
using matechat.feature;
using matechat.ui;
using matechat.util;

namespace matechat
{
    public class Core : MelonMod
    {
        private static List<Feature> features;
        private MenuManager menuManager;
        private bool isInitialized;

        private static IAIEngine aiEngine;
        private static IAudioProcessor audioEngine;


        public override void OnApplicationStart()
        {
            try
            {
                // Initialize the configuration
                Config.Initialize();

                // Initialize the AI engine based on the config
                InitializeAIEngine();

                // initialize the Audio engine based on the config
                InitializeAudioEngine();


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

        private static void InitializeAIEngine()
        {
            try
            {
                switch (Config.ENGINE_TYPE.Value)
                {
                    case "Cloudflare":
                        aiEngine = new CloudflareUtil();
                        MelonLogger.Msg("Using Cloudflare engine.");
                        break;

                    case "OpenRouter":
                        aiEngine = new OpenRouterEngine();
                        MelonLogger.Msg("Using OpenRouter engine.");
                        break;

                    case "OpenAI":
                        aiEngine = new OpenAIUtil();
                        MelonLogger.Msg("Using OpenAI engine.");
                        break;

                    case "OpenAICompatible":
                        aiEngine = new OpenAIUtil();
                        MelonLogger.Msg("Using OpenAI(OpenAICompatible) engine.");
                        break;

                    default:
                        MelonLogger.Error(
                            $"Unsupported AI engine type: {Config.ENGINE_TYPE.Value}");
                        throw new System.Exception("Invalid AI engine configuration.");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to initialize AI engine: {ex.Message}");
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


        public static void ReloadAIEngine()
        {
            try
            {
                InitializeAIEngine();
                MelonLogger.Msg("AI engine reloaded successfully.");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to reload AI engine: {ex.Message}");
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

        public static IAIEngine GetAIEngine()
        {
            if (aiEngine == null)
            {
                throw new System.Exception("AI engine is not initialized.");
            }
            return aiEngine;
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
