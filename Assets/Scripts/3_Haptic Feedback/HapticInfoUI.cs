using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HapticInfoUI : MonoBehaviour
{
    public Canvas canvas;
    public HapticSim hapticSim;
    public TextMeshProUGUI statsText;
    public Scrollbar amplitudeScrollbar;

    void Start()
    {
        if (canvas != null && !canvas.gameObject.activeSelf) canvas.gameObject.SetActive(true);
    }

    public void UpdateInfoUI(
        string namaBahan, float kekakuan, float penetrasi,
        float gayaNoCapped, float gaya, float gayaMaksimum,
        float amplitudo, bool pseudoHaptics)
    {
        if (hapticSim == null || statsText == null || amplitudeScrollbar == null) return;

        statsText.text = $"Bahan                 : {namaBahan}   (k = {kekakuan:F0} N/m)\n" +
                         $"Penetrasi            : {penetrasi:F4} m\n" +
                         $"Gaya asli            : {gayaNoCapped:F2} N   (maks {gayaMaksimum:F0} N)\n" +
                         $"Gaya simulasi    : {gaya:F2} N   (maks {gayaMaksimum:F0} N)\n" +
                         $"Amplitudo           : {amplitudo:F2}\n" +
                         $"Pseudo-haptics  : {(pseudoHaptics ? "aktif" : "nonaktif")}";

        ColorBlock colors = amplitudeScrollbar.colors;
        colors.disabledColor = Color.Lerp(Color.green, Color.red, amplitudo);
        amplitudeScrollbar.colors = colors;

        amplitudeScrollbar.size = amplitudo;
    }
}
