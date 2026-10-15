public static class ProjectConstants
{
    public const string NIM = "2310511069";

    // HEX
    public const string WARNA_UTAMA = "#511069";

    // Satuan millisecond (ms) 
    // Mathf.Floor(55f + (69f / 5f)) = 68 ms
    public const float IPD_UJI = 68;

    // Satuan meter (m)
    // 1f + (6 % 4) = 3 m
    public const float JARAK_KONVERGENSI = 3;

    // Satuan Newton/meter (N/m)
    // 100f + 0 * 200 = 100 N/m
    public const float KEKAKUAN_BAHAN = 100f;

    // Satuan millimeter (mm) = 6 mm
    // 5f + 1f
    public const float NOISE_TRACKER = 6f;

    // Satuan meter/second (m/s)
    // 0.8f + 1f * 0.1f = 0.9 m/s
    public const float AMBANG_GESTUR = 0.9f;

    public const string TEMA_APLIKASI = "Galeri Pengenalan Kampus";

    public const float MIN_KEKAKUAN = 0f;
    public const float MAX_KEKAKUAN = 2000f;

    public const float MIN_REDAMAN = 0f;
    public const float MAX_REDAMAN = 200f;
    // 200n +
}
