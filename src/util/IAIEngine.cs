using System;
using System.Collections;

namespace matechat.util
{
    namespace matechat.util
    {
        public interface IAIEngine
        {
            IEnumerator SendRequest(string userMessage, string systemPrompt, System.Action<string, string> callback);
            IEnumerator TestEngine(System.Action<bool, string> callback);
        }
    }
}
