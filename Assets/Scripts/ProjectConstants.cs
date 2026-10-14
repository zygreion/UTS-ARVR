using System;
using UnityEngine;

public class ProjectConstants : MonoBehaviour
{
    public static string NIM = "2310511069";

    // HEX
    public static string WARNA_UTAMA = "#511069";

    // Satuan millisecond (ms) = 68 ms
    public static float IPD_UJI = Mathf.Floor(55f + (69f / 5f));

    // Satuan meter (m) = 3 m
    public static float JARAK_KONVERGENSI = 1f + (6 % 4);

    // Satuan Newton/meter (N/m) = 100 N/m
    public static float KEKAKUAN_BAHAN = 100f + 0 * 200;

    // Satuan millimeter (mm) = 6 mm
    public static float NOISE_TRACKER = 5f + 1;

    // Satuan meter/second (m/s) = 0.9 m/s
    public static float AMBANG_GESTUR = 0.8f + 1 * 0.1f;

    public static string TEMA_APLIKASI = "Galeri Pengenalan Kampus";
}
