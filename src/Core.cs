using MelonLoader;
using UnityEngine;

using matechat.sdk.Feature;

using matechat.feature;
using matechat.ui;

namespace matechat
{
    public class Core : MelonMod
    {
        private List<Feature> features;
        private MenuManager menuManager;

        public override void OnEarlyInitializeMelon()
        {
            Config.Initialize();

            menuManager = new MenuManager();

            features = new List<Feature>();
            features.Add(new ChatFeature());
        }

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnLateUpdate()
        {
            foreach (var feature in features)
            {
                if (Input.GetKeyDown(feature.Keybind))
                {
                    feature.Toggle();
                }
            }
        }

        public override void OnGUI()
        {
            foreach (var feature in features)
            {
                if (feature.IsEnabled && feature is ChatFeature chatFeature)
                {
                    chatFeature.DrawGUI();
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            MelonCoroutines.Start(menuManager.WaitForMenu());
        }
    }
}
