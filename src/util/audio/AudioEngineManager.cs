using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matechat.util
{
    public class AudioEngineManager
    {
        private readonly Dictionary<string, IAudioEngine> _engines;
        private readonly string _defaultEngine;

        public AudioEngineManager(string defaultEngine, params (string Name, IAudioEngine Engine)[] engines)
        {
            _engines = new Dictionary<string, IAudioEngine>(StringComparer.OrdinalIgnoreCase);
            _defaultEngine = defaultEngine;

            foreach (var (name, engine) in engines)
            {
                _engines[name] = engine;
            }
        }        public async Task<string> ProcessAudioAsync(string text, string engineName = null)
        {
            engineName ??= _defaultEngine;

            if (!_engines.TryGetValue(engineName, out var engine))
            {
                throw new InvalidOperationException($"Engine '{engineName}' not found. Available engines: {string.Join(", ", _engines.Keys)}");
            }

            // Just call the interface method:
            return await engine.ProcessAudioAsync(text);
        }

    }
}
