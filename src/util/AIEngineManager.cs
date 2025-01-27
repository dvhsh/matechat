using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matechat.util
{
    public class AIEngineManager
    {
        private readonly Dictionary<string, IAIEngine> _engines;
        private readonly string _defaultEngine;

        public AIEngineManager(string defaultEngine, params (string Name, IAIEngine Engine)[] engines)
        {
            _engines = new Dictionary<string, IAIEngine>(StringComparer.OrdinalIgnoreCase);
            _defaultEngine = defaultEngine;

            foreach (var (name, engine) in engines)
            {
                _engines[name] = engine;
            }
        }

        public async Task<string> SendRequestAsync(string prompt, string engineName = null, string model = null, string systemprompt = null)
        {
            engineName ??= _defaultEngine;

            if (!_engines.TryGetValue(engineName, out var engine))
            {
                throw new InvalidOperationException($"Engine '{engineName}' not found. Available engines: {string.Join(", ", _engines.Keys)}");
            }

            return await engine.SendRequestAsync(prompt, model, systemprompt);
        }

        public async Task<bool> TestEngineAsync(string engineName = null)
        {
            engineName ??= _defaultEngine;

            if (!_engines.TryGetValue(engineName, out var engine))
            {
                throw new InvalidOperationException($"Engine '{engineName}' not found. Available engines: {string.Join(", ", _engines.Keys)}");
            }

            return await engine.TestConnectionAsync();
        }
        public async Task<string> ProcessAudioAsync(string text, string engineName = null)
        {
            engineName ??= _defaultEngine;

            if (!_engines.TryGetValue(engineName, out var engine))
            {
                throw new InvalidOperationException($"Engine '{engineName}' not found. Available engines: {string.Join(", ", _engines.Keys)}");
            }

            if (!(engine is TTSEngine ttsEngine))
            {
                throw new InvalidOperationException($"Engine '{engineName}' is not a valid TTS engine.");
            }

            return await ttsEngine.ProcessAudioAsync(text);
        }
    }
}
