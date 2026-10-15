// =============================================================================
//  MaterialHaptik.cs
//  Mata Kuliah : Realitas Virtual dan Realitas Augmentasi (SE.1) - INF124312
//  Pertemuan 4 : Force Feedback Simulation dan Haptic Device
//
//  Dipasang pada objek uji untuk memberinya sifat bahan yang berbeda-beda.
//  Nilai di sini dibaca oleh HapticFeedbackLab saat probe menyentuh objek.
//
//  Nilai acuan untuk latihan:
//    Busa   : kekakuan  60  redaman  2
//    Karet  : kekakuan 250  redaman  8
//    Kayu   : kekakuan 900  redaman  4
//    Logam  : kekakuan 1800 redaman  2
// =============================================================================

using UnityEngine;
using static ProjectConstants;

public class MaterialHaptik : MonoBehaviour
{
    [Tooltip("Nama bahan, ditampilkan pada panel data di layar.")]
    public string namaBahan = "Bahan";

    [Tooltip("Kekakuan bahan dalam newton per meter. Besar = terasa keras.")]
    [Range(MIN_KEKAKUAN, MAX_KEKAKUAN)] public float kekakuan = KEKAKUAN_BAHAN;

    [Tooltip("Redaman bahan. Besar = terasa seperti menekan benda basah atau kental.")]
    [Range(MIN_REDAMAN, MAX_REDAMAN)] public float redaman = 5f;
}
