using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matechat.util
{
    /// <summary>
    /// Basic interface for any streaming audio player.
    /// // This allows the engine to push raw PCM data in chunks.
    /// </summary>
    public interface IAudioStreamPlayer
    {
        /// <summary>
        /// Initialize playback with a given sample rate and channel count.
        /// </summary>
        void Initialize(int sampleRate, int channels);

        /// <summary>
        /// Enqueue or push PCM data. Should handle ring buffer or incremental writing.
        /// </summary>
        void PushAudioData(byte[] pcmData);

        /// <summary>
        /// Stop and dispose the player.
        /// </summary>
        void Stop();
    }
}
