using UnityEngine;

namespace matechat.sdk.Feature
{
    public abstract class Feature
    {
        public string Name { get; protected set; }
        public KeyCode Keybind { get; protected set; }
        public bool IsEnabled { get; protected set; }

        protected Feature(string name, KeyCode keybind)
        {
            Name = name;
            Keybind = keybind;
            IsEnabled = false;
        }

        public virtual void Toggle()
        {
            IsEnabled = !IsEnabled;
        }
    }
}
