// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace JPEmuRenamer.Emulation;

/// <summary>
/// コアが出す音（16bitステレオ）を、出力デバイスの周波数に変換して再生する。
/// 音ずれ補正を有効にすると、バッファの残量に合わせて変換の比率をわずかに変え、音切れや遅れの蓄積を防ぐ。
/// </summary>
public sealed class AudioOutput : IDisposable, IWaveProvider
{
    private readonly object gate = new();
    private float[] ring;
    private int readPos, writePos, count;
    private IWavePlayer? player;
    private double inputRate;
    private double frac;
    private float lastL, lastR;

    public WaveFormat WaveFormat { get; }
    public int OutputRate { get; }
    public float Volume { get; set; } = 1f;
    public bool RateControl { get; set; } = true;
    public int TargetLatencyMs { get; }
    public string DeviceDescription { get; } = "";

    public AudioOutput(double inputRate, string deviceId, int sampleRate, bool exclusive, int latencyMs)
    {
        this.inputRate = inputRate > 0 ? inputRate : 44100;
        TargetLatencyMs = Math.Clamp(latencyMs, 16, 500);

        MMDevice? device = null;
        try
        {
            using var en = new MMDeviceEnumerator();
            device = string.IsNullOrEmpty(deviceId)
                ? en.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                : en.GetDevice(deviceId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        int rate = sampleRate is 44100 or 48000 ? sampleRate : 48000;
        WasapiOut? wasapi = null;
        if (device != null)
        {
            if (!exclusive)
            {
                // 共有モードはデバイスのミキサーの周波数に合わせると変換が1回で済む
                try { rate = device.AudioClient.MixFormat.SampleRate; } catch { }
            }
            DeviceDescription = device.FriendlyName;
        }

        OutputRate = rate;
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(rate, 2);
        // リングバッファは目標遅延の4倍（最低0.5秒）
        ring = new float[Math.Max(rate / 2, rate * TargetLatencyMs * 4 / 1000) * 2];

        if (device != null)
        {
            try
            {
                wasapi = new WasapiOut(device, exclusive ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, true, Math.Max(10, TargetLatencyMs / 2));
                wasapi.Init(this);
                wasapi.Play();
                player = wasapi;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                wasapi?.Dispose();
                player = null;
            }
        }
        if (player == null)
        {
            // 排他モードが使えない・WASAPIが失敗したときは WaveOut で鳴らす
            try
            {
                var wo = new WaveOutEvent { DesiredLatency = Math.Max(40, TargetLatencyMs) };
                wo.Init(this);
                wo.Play();
                player = wo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }

    public bool IsPlaying => player != null;

    public void SetInputRate(double rate)
    {
        if (rate > 0) lock (gate) inputRate = rate;
    }

    /// <summary>バッファに溜まっている音の長さ（ミリ秒）</summary>
    public double BufferedMs
    {
        get { lock (gate) return count / 2 * 1000.0 / OutputRate; }
    }

    public void Clear()
    {
        lock (gate) { readPos = writePos = count = 0; }
    }

    /// <summary>コアの音（16bit、左右交互）を受け取り、変換してバッファに積む</summary>
    public unsafe void Write(short* frames, int frameCount)
    {
        if (frameCount <= 0) return;
        lock (gate)
        {
            double ratio = inputRate / OutputRate;
            if (RateControl)
            {
                // 残量が目標より多ければ少し速く（出力を減らす）、少なければ少し遅く
                double target = TargetLatencyMs / 1000.0 * OutputRate * 2;
                double delta = Math.Clamp((count - target) / Math.Max(1, target), -1, 1);
                ratio *= 1 + 0.005 * delta;
            }

            for (int i = 0; i < frameCount; i++)
            {
                float l = frames[i * 2] / 32768f, r = frames[i * 2 + 1] / 32768f;
                // 直前のサンプルとの間を直線で補間して出力の周波数に合わせる
                while (frac < 1.0)
                {
                    Push(lastL + (l - lastL) * (float)frac, lastR + (r - lastR) * (float)frac);
                    frac += ratio;
                }
                frac -= 1.0;
                lastL = l;
                lastR = r;
            }
        }
    }

    private void Push(float l, float r)
    {
        if (count + 2 > ring.Length)
        {
            // 溢れたら古いものを捨てる
            readPos = (readPos + 2) % ring.Length;
            count -= 2;
        }
        ring[writePos] = l;
        ring[writePos + 1] = r;
        writePos = (writePos + 2) % ring.Length;
        count += 2;
    }

    /// <summary>再生側（NAudio）から呼ばれる</summary>
    public int Read(byte[] buffer, int offset, int bytes)
    {
        int samples = bytes / 4;
        float vol = Volume;
        lock (gate)
        {
            for (int i = 0; i < samples; i++)
            {
                float v = 0;
                if (count > 0)
                {
                    v = ring[readPos] * vol;
                    readPos = (readPos + 1) % ring.Length;
                    count--;
                }
                BitConverter.TryWriteBytes(new Span<byte>(buffer, offset + i * 4, 4), v);
            }
        }
        return bytes;
    }

    public void Dispose()
    {
        try { player?.Stop(); } catch { }
        player?.Dispose();
        player = null;
    }

    /// <summary>出力デバイスの一覧（ID, 名前）</summary>
    public static List<(string id, string name)> Devices()
    {
        var list = new List<(string, string)>();
        try
        {
            using var en = new MMDeviceEnumerator();
            foreach (var d in en.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                list.Add((d.ID, d.FriendlyName));
        }
        catch { }
        return list;
    }
}
