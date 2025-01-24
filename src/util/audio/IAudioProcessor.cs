using System.Collections;

namespace matechat.util
{
    public interface IAudioProcessor
    {
        IEnumerator ProcessAudio(string text, System.Action<string, string> callback);
    }
}
