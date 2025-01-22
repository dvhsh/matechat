using System.Collections;

namespace matechat.util
{
    public interface IAIEngine
    {
        Task<string> SendRequestAsync(string prompt, string model = null, string systemprompt = null);
        Task<bool> TestConnectionAsync(string model = null);
    }
}
