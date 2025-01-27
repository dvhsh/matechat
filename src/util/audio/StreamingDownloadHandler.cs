using System;
using UnityEngine.Networking;

namespace matechat.util
{
    /// <summary>
    /// A custom DownloadHandler that triggers a callback for every received chunk.
    /// // This is essential for real-time streaming in Unity.
    /// </summary>
    public class StreamingDownloadHandler : DownloadHandler
    {
        private readonly Action<byte[]> _onChunkReceived;

        public StreamingDownloadHandler(Action<byte[]> callback)
        {
            _onChunkReceived = callback;
        }

        protected bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0)
                return false;

            var chunk = new byte[dataLength];
            Buffer.BlockCopy(data, 0, chunk, 0, dataLength);

            _onChunkReceived?.Invoke(chunk);
            return true;
        }

        public override float GetProgress()
        {
            // We can't reliably calculate progress in streaming
            return 0f;
        }

        public override void CompleteContent()
        {
            // Called when download is fully complete
            base.CompleteContent();
        }

        public override string GetText()
        {
            // Not applicable for binary streaming
            return null;
        }
    }
}
