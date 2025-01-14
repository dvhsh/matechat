using MelonLoader;
using UnityEngine;

using System.Collections;

using matechat.sdk.Feature;

using matechat.feature;
using matechat.ui;

namespace matechat
{
    public class Core : MelonMod
    {
        private static List<Feature> features;
        private MenuManager menuManager;
        private bool isInitialized;

        public override void OnApplicationStart()
        {
            try
            {
                Config.Initialize();
                features = new List<Feature>();
                features.Add(new ChatFeature());

                menuManager = new MenuManager();
                isInitialized = true;

                Config.ReloadConfig();
                LoggerInstance.Msg("Initialized.");

                // wait a second before starting mod (@FIX)
                MelonCoroutines.Start(DelayedMenuInit());
            }
            catch (System.Exception ex)
            {
                LoggerInstance.Error($"Failed to initialize: {ex}");
            }
        }

        private IEnumerator DelayedMenuInit()
        {
            yield return new WaitForSeconds(1f);
            MelonCoroutines.Start(menuManager.WaitForMenu());
        }

        // @TODO exchange this for a func to get all features then sort/filter
        public static ChatFeature GetChatFeature()
        {
            return features?.FirstOrDefault(f => f is ChatFeature) as ChatFeature;
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
