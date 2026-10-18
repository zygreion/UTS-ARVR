using UnityEngine;

/// <summary>
/// ProceduralAudio: Menghasilkan klip audio spasial 3D sintetis secara langsung via kode.
/// Menggunakan sintesis gelombang matematis (sine wave, noise, dan envelope peluruhan),
/// sehingga proyek 100% mandiri dan tidak memerlukan dependensi file audio eksternal.
/// </summary>
public static class ProceduralAudio
{
    private static AudioClip _cachedChime;
    private static AudioClip _cachedWhoosh;
    private static AudioClip _cachedSwitch;
    private static AudioClip _cachedFootstep;

    /// <summary>
    /// Menghasilkan klip lonceng harmonis (Chime) untuk umpan balik klik/interaksi maket.
    /// Frekuensi 880 Hz (A5) dengan overtone 1320 Hz (E6).
    /// </summary>
    public static AudioClip GetChimeClip()
    {
        if (_cachedChime != null) return _cachedChime;

        int sampleRate = 44100;
        float duration = 0.35f;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-8f * t); // Peluruhan eksponensial halus
            float wave = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.7f +
                         Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.3f;
            data[i] = wave * env * 0.5f;
        }

        _cachedChime = AudioClip.Create("Proc_Chime", samples, 1, sampleRate, false);
        _cachedChime.SetData(data, 0);
        return _cachedChime;
    }

    /// <summary>
    /// Menghasilkan suara sapuan frekuensi (Whoosh) saat mengaktifkan mode inspeksi / rotasi maket.
    /// </summary>
    public static AudioClip GetWhooshClip()
    {
        if (_cachedWhoosh != null) return _cachedWhoosh;

        int sampleRate = 44100;
        float duration = 0.35f;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float freq = Mathf.Lerp(220f, 660f, progress);
            float env = Mathf.Sin(Mathf.PI * progress); // Kurva amplifikasi berbentuk lonceng
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            data[i] = wave * env * 0.45f;
        }

        _cachedWhoosh = AudioClip.Create("Proc_Whoosh", samples, 1, sampleRate, false);
        _cachedWhoosh.SetData(data, 0);
        return _cachedWhoosh;
    }

    /// <summary>
    /// Menghasilkan suara klik saklar (Switch Click) untuk perubahan mode pencahayaan galeri.
    /// </summary>
    public static AudioClip GetSwitchClip()
    {
        if (_cachedSwitch != null) return _cachedSwitch;

        int sampleRate = 44100;
        float duration = 0.08f;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-45f * t);
            float wave = Mathf.Sin(2f * Mathf.PI * 1400f * t);
            data[i] = wave * env * 0.6f;
        }

        _cachedSwitch = AudioClip.Create("Proc_Switch", samples, 1, sampleRate, false);
        _cachedSwitch.SetData(data, 0);
        return _cachedSwitch;
    }

    /// <summary>
    /// Menghasilkan suara hentakan redam (Footstep) saat navigasi viewer bergerak di lantai galeri.
    /// </summary>
    public static AudioClip GetFootstepClip()
    {
        if (_cachedFootstep != null) return _cachedFootstep;

        int sampleRate = 44100;
        float duration = 0.12f;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-28f * t);
            float wave = Mathf.Sin(2f * Mathf.PI * 95f * t) + (Random.value * 2f - 1f) * 0.12f;
            data[i] = wave * env * 0.35f;
        }

        _cachedFootstep = AudioClip.Create("Proc_Footstep", samples, 1, sampleRate, false);
        _cachedFootstep.SetData(data, 0);
        return _cachedFootstep;
    }
}
