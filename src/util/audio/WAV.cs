using System;
using UnityEngine;

public class WAV
{
    public float[] LeftChannel { get; private set; }
    public int SampleCount { get; private set; }
    public int Frequency { get; private set; }

    public WAV(byte[] wav)
    {
        if (BitConverter.ToUInt32(wav, 0) != 0x46464952) // "RIFF"
            throw new Exception("Invalid WAV file");

        Frequency = BitConverter.ToInt32(wav, 24);

        int dataIndex = 12;
        while (BitConverter.ToUInt32(wav, dataIndex) != 0x61746164) // "data"
        {
            dataIndex += 4;
            int chunkSize = BitConverter.ToInt32(wav, dataIndex);
            dataIndex += 4 + chunkSize;
        }
        dataIndex += 8;

        SampleCount = (wav.Length - dataIndex) / 2;

        short[] pcmData = new short[SampleCount];
        for (int i = 0; i < SampleCount; i++)
            pcmData[i] = BitConverter.ToInt16(wav, dataIndex + i * 2);

        // Normalize (-1.0 ~ 1.0)
        LeftChannel = new float[SampleCount];
        for (int i = 0; i < SampleCount; i++)
            LeftChannel[i] = pcmData[i] / 32768f;
    }
    public void SaveWav(string path)
    {
        try
        {
            // generate parent directory
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fs = new FileStream(path, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // WAV header
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + SampleCount * 2);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });

                // fmt sub chunk
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1); // Mono
                writer.Write(Frequency);
                writer.Write(Frequency * 2);
                writer.Write((short)2);
                writer.Write((short)16);

                // data sub chunk
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(SampleCount * 2);

                // PCM data
                foreach (var sample in LeftChannel)
                {
                    short pcmValue = (short)(sample * 32767);
                    writer.Write(pcmValue);
                }
            }

            Debug.Log($"[WAV] Successfully saved to: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[WAV] Failed to save WAV file: {ex.Message}");
        }
    }
}

