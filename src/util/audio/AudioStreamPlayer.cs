using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;

namespace matechat.util
{
    [RegisterTypeInIl2Cpp]
    /// <summary>
    /// A simple ring buffer based streaming player using OnAudioFilterRead.
    /// // Real-time PCM data is enqueued, then consumed in OnAudioFilterRead.
    /// // This avoids rewriting a full AudioClip; instead we fill Unity's DSP buffer directly.
    /// </summary>
    public class AudioStreamPlayer : MonoBehaviour
    {
        private ConcurrentQueue<float> _sampleQueue = new ConcurrentQueue<float>();

        private int _sampleRate = 48000;
        private int _channels = 1;
        private bool _isPlaying = false;

        private AudioSource _source;

        // This buffer can be tuned based on expected streaming speed or your latency requirements.
        // It's how many samples we try to keep at max. 
        private const int MAX_BUFFER_SAMPLES = 48000 * 4; // e.g., up to 4 seconds worth at 48kHz, mono

        // Thread-safety note:
        // In many cases, EnqueueChunk will happen on the main thread 
        // and OnAudioFilterRead on Unity's audio thread. 
        // So using a ConcurrentQueue is recommended.

        public AudioStreamPlayer(IntPtr ptr) : base(ptr)
        {
            // 2) Il2Cpp requires this constructor
        }

        /// <summary>
        /// Initialize the streaming player with sample rate and channel count.
        /// </summary>
        public void Initialize(int sampleRate, int channels)
        {
            _sampleRate = sampleRate;
            _channels = channels;

            // Create an AudioSource on the same GameObject
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            // We won't rely on AudioClip data, we'll use OnAudioFilterRead instead.

            _source.Stop();
            _source.clip = null; // We don't set a clip, since we're handling data in OnAudioFilterRead.
            _isPlaying = false;
        }

        /// <summary>
        /// Enqueue PCM data in 16-bit raw bytes (little-endian).
        /// Convert them to float samples and push into the ring buffer queue.
        /// </summary>
        public void EnqueueChunk(Il2CppStructArray<byte> chunk)
        {
            if (chunk == null || chunk.Length == 0)
                return;

            // Convert PCM16 -> float
            // chunk is presumably 16-bit little-endian PCM, with channel layout that 
            // we assume matches _channels. For simplicity, we just treat it as interleaved PCM.
            int sampleCount = chunk.Length / 2; // 16-bit = 2 bytes per sample
            for (int i = 0; i < sampleCount; i++)
            {
                short value = (short)((chunk[i * 2 + 1] << 8) | chunk[i * 2]);
                float sample = value / 32768f;
                _sampleQueue.Enqueue(sample);
            }

            // Optionally, if the queue is too large, we could discard older samples 
            // (depending on your logic). For instance:
            while (_sampleQueue.Count > MAX_BUFFER_SAMPLES)
            {
                _sampleQueue.TryDequeue(out _);
            }

            // Start playback if not yet playing
            if (!_isPlaying)
            {
                _isPlaying = true;
                _source.Play();
            }
        }

        /// <summary>
        /// Stop playback and clear the buffer. This is also called by the streaming engine upon cancel or cleanup.
        /// </summary>
        public void Stop()
        {
            if (_source != null && _source.isPlaying)
            {
                _source.Stop();
            }
            _isPlaying = false;
            _sampleQueue.Clear();
        }

        /// <summary>
        /// Unity's audio thread callback. This is where we fill the DSP buffer with samples from our queue.
        /// // data.Length = framesRequested * channels
        /// // data is a float[] that Unity will mix out to the final output.
        /// </summary>
        private void OnAudioFilterRead(float[] data, int channels)
        {
            // If no data is playing, fill with silence
            if (!_isPlaying || _sampleQueue.Count == 0)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0f;
                }
                return;
            }

            // Fill 'data' from our queue
            int neededSamples = data.Length;
            for (int i = 0; i < neededSamples; i++)
            {
                if (_sampleQueue.TryDequeue(out float sample))
                {
                    data[i] = sample;
                }
                else
                {
                    // If we've run out of samples, fill the rest with 0
                    data[i] = 0f;
                }
            }
        }
    }
}
