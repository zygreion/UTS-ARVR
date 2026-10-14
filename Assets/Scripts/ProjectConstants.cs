using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectConstants : MonoBehaviour
{
    public static string NIM = "2310511069";

    // HEX
    public static string WARNA_UTAMA = "#511069";

    // Satuan millisecond (ms)
    public static float IPD_UJI = (float)(55f + Math.Floor(69f / 5f));

    // Satuan meter (m)
    public static float JARAK_KONVERGENSI = 1f + (6 % 4);

    // Satuan Newton/meter (N/m)
    public static float KEKAKUAN_BAHAN = 100f + 0 * 200;

    // Satuan millimeter (mm)
    public static float NOISE_TRACKER = 5f + 1;

    // Satuan meter/second (m/s)
    public static float AMBANG_GESTUR = 0.8f + 1 * 0.1f;

    public static string TEMA_APLIKASI = "Galeri Pengenalan Kampus";
}
