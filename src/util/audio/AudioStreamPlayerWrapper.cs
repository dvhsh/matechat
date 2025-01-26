using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace matechat.util
{
    public class AudioStreamPlayerWrapper : IAudioStreamPlayer
    {
        private AudioStreamPlayer _unityPlayer;
        private GameObject _playerObj;

        public AudioStreamPlayerWrapper(GameObject owner)
        {
            _playerObj = owner;
            _unityPlayer = _playerObj.AddComponent<AudioStreamPlayer>();
        }

        public void Initialize(int sampleRate, int channels)
        {
            _unityPlayer.Initialize(sampleRate, channels);
        }

        public void PushAudioData(byte[] pcmData)
        {
            // Convert to Il2Cpp array if needed
            var il2cppData = new Il2CppStructArray<byte>(pcmData.Length);
            Buffer.BlockCopy(pcmData, 0, il2cppData, 0, pcmData.Length);

            _unityPlayer.EnqueueChunk(il2cppData);
        }

        public void Stop()
        {
            if (_unityPlayer != null)
            {
                _unityPlayer.Stop();
                UnityEngine.Object.Destroy(_playerObj);
            }
        }
    }
}
