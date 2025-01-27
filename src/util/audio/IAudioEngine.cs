using System.Collections;

namespace matechat.util
{
    public interface IAudioEngine
    {
        IEnumerator ProcessAudio(string text, Action<string, string> callback);
        Task<string> ProcessAudioAsync(string text);
        void Cancel(); // 스트리밍 엔진을 위한 취소 메서드 추가
    }
}
