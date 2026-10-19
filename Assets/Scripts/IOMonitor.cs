using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// IOMonitor: Komponen inti pemantau jalur Input–Output (Sub-CPMK 1).
/// Membaca dan menampilkan secara real-time di layar:
/// 1. Posisi dan orientasi viewer (6-DoF).
/// 2. Status sekurang-kurangnya 3 perangkat input yang berbeda (Keyboard, Mouse, Gamepad/Virtual Controller).
/// 3. Membalas setiap aksi input dengan sekurang-kurangnya 2 kanal output berbeda (Visual dan Audio Spasial 3D).
/// </summary>
[DisallowMultipleComponent]
public class IOMonitor : MonoBehaviour
{
    [Header("=== REFERENSI VIEWER ===")]
    [Tooltip("Transform viewer / kamera utama.")]
    public Transform viewerTransform;

    [Tooltip("Controller pergerakan viewer.")]
    public ViewerController viewerController;

    [Header("=== OUTPUT KANAL 1: VISUAL (UI & OBJEK) ===")]
    [Tooltip("Komponen TextMeshProUGUI pada Canvas layar untuk menampilkan status real-time.")]
    public TextMeshProUGUI statusText;

    [Tooltip("Objek utama maket gedung kampus yang memakai material berwarna WARNA_UTAMA (#511069).")]
    public Transform interactiveMaket;

    [Tooltip("Papan informasi di dekat maket untuk menampilkan detail objek terpilih.")]
    public TMP_Text infoBoardText;

    [Tooltip("Lampu sorot galeri pameran.")]
    public Light exhibitionSpotlight;

    [Tooltip("Lampu ambient directional light.")]
    public Light galleryDirectionalLight;

    [Tooltip("Lampu plafon utama pencahayaan ruangan galeri.")]
    public Light galleryRoomLight;

    [Header("=== OUTPUT KANAL 2: AUDIO SPASIAL 3D ===")]
    [Tooltip("AudioSource 3D untuk memutar umpan balik audio.")]
    public AudioSource spatialAudioSource;

    [Header("=== STATUS & INDIKATOR RUNTIME ===")]
    [Tooltip("Status mode pameran / rotasi maket (diaktifkan via tombol Spasi).")]
    public bool isTurntableActive = false;

    [Tooltip("Status mode pencahayaan galeri (diubah via tombol E).")]
    public bool isNightShowcaseMode = false;

    // Komponen internal & cache
    private Renderer _maketRenderer;
    private Color _originalMaketColor;
    private readonly Dictionary<Transform, Vector3> _baseScales = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, Coroutine> _activePulses = new Dictionary<Transform, Coroutine>();
    private readonly StringBuilder _sb = new StringBuilder(1024);

    // Pencatat status kanal keluaran terakhir
    private string _lastActionName = "Sistem Siap (Gunakan WASD, Mouse, Spasi, atau E)";
    private string _lastVisualOutput = "Menampilkan Galeri Kampus";
    private string _lastAudioOutput = "Audio Siap";

    // Klip audio prosedural sintetis (tanpa file eksternal)
    private AudioClip _chimeClip;
    private AudioClip _whooshClip;
    private AudioClip _switchClip;
    private AudioClip _footstepClip;

    private void Awake()
    {
        if (viewerTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null) viewerTransform = mainCam.transform;
            else viewerTransform = transform;
        }

        if (viewerController == null && viewerTransform != null)
        {
            viewerController = viewerTransform.GetComponent<ViewerController>();
        }

        if (spatialAudioSource == null)
        {
            spatialAudioSource = gameObject.GetComponent<AudioSource>();
            if (spatialAudioSource == null) spatialAudioSource = gameObject.AddComponent<AudioSource>();
            spatialAudioSource.spatialBlend = 1.0f; // 3D Spatial Audio penuh
            spatialAudioSource.playOnAwake = false;
        }

        // Cache klip audio sintetis mandiri
        _chimeClip = ProceduralAudio.GetChimeClip();
        _whooshClip = ProceduralAudio.GetWhooshClip();
        _switchClip = ProceduralAudio.GetSwitchClip();
        _footstepClip = ProceduralAudio.GetFootstepClip();

        if (interactiveMaket != null)
        {
            _maketRenderer = interactiveMaket.GetComponentInChildren<Renderer>();
            if (_maketRenderer != null && _maketRenderer.material != null)
            {
                _originalMaketColor = _maketRenderer.material.color;
            }
        }
    }

    private void Start()
    {
        if (interactiveMaket != null)
        {
            // Reset skala ke normal (Vector3.one) jika sebelumnya sempat membesar akibat klik lama
            interactiveMaket.localScale = Vector3.one;
            _baseScales[interactiveMaket] = Vector3.one;
        }

        ApplyLightingMode();
    }

    private void OnEnable()
    {
        if (viewerController != null)
        {
            viewerController.OnStepTaken += HandleFootstep;
        }
    }

    private void OnDisable()
    {
        if (viewerController != null)
        {
            viewerController.OnStepTaken -= HandleFootstep;
        }
    }

    private void Update()
    {
        HandleInputActions();
        UpdateTurntableAnimation();
        RenderMonitoringHUD();
    }

    /// <summary>
    /// Menangani aksi input interaktif dan memicu respon pada kanal keluaran ganda (Visual & Audio).
    /// </summary>
    private void HandleInputActions()
    {
        // ---------------------------------------------------------------------
        // AKSI INPUT 1: Klik Kiri Mouse (Raycast interaksi pada objek 3D galeri)
        // Kanal Output 1 (Visual) : Scale bounce terkendali, highlight warna, update papan info
        // Kanal Output 2 (Audio)  : Lonceng spasial 3D (Spatial Chime)
        // ---------------------------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main != null ? Camera.main.ScreenPointToRay(Input.mousePosition) : new Ray(viewerTransform.position, viewerTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 25f))
            {
                Transform targetTransform = hit.collider.transform;
                string hitName = hit.collider.gameObject.name;

                // Jika yang diklik adalah bagian dari maket gedung utama, targetkan maket utama secara utuh
                if (interactiveMaket != null && (targetTransform == interactiveMaket || targetTransform.IsChildOf(interactiveMaket)))
                {
                    targetTransform = interactiveMaket;
                    hitName = "Maket Gedung Kampus Rektorat";
                }

                TriggerInteractionFeedback(targetTransform, hit.point, hitName);
            }
            else
            {
                // Jika klik ke ruang kosong, picu interaksi pada maket utama langsung
                if (interactiveMaket != null)
                {
                    TriggerInteractionFeedback(interactiveMaket, interactiveMaket.position, "Maket Gedung Kampus Rektorat");
                }
            }
        }

        // ---------------------------------------------------------------------
        // AKSI INPUT 2: Tombol Spasi (Toggle Rotasi Turntable Mode Pameran Maket)
        // Kanal Output 1 (Visual) : Maket berputar otomatis + lampu sorot berganti intensitas
        // Kanal Output 2 (Audio)  : Suara sapuan frekuensi transisi mekanis (Whoosh)
        // ---------------------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isTurntableActive = !isTurntableActive;
            _lastActionName = "Tombol Spasi [Space Key] Ditekan";
            _lastVisualOutput = isTurntableActive ? "Rotasi Pameran AKTIF, Lampu Sorot Terang" : "Rotasi Pameran BERHENTI, Lampu Normal";

            if (exhibitionSpotlight != null)
            {
                exhibitionSpotlight.intensity = isTurntableActive ? 2.5f : 1.2f;
                exhibitionSpotlight.color = isTurntableActive ? new Color(1f, 0.95f, 0.8f) : Color.white;
            }

            PlaySpatialSound(_whooshClip, interactiveMaket != null ? interactiveMaket.position : viewerTransform.position, 1.0f);
            _lastAudioOutput = "Audio Whoosh Transisi Mode (3D Spasial)";
        }

        // ---------------------------------------------------------------------
        // AKSI INPUT 3: Tombol E (Saklar Mode Pencahayaan Galeri: Siang vs Malam)
        // Kanal Output 1 (Visual) : Ambient galeri berubah (Daylight vs Night Spotlight)
        // Kanal Output 2 (Audio)  : Suara saklar listrik tajam (Switch Click)
        // ---------------------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.E))
        {
            isNightShowcaseMode = !isNightShowcaseMode;
            _lastActionName = "Tombol E [Interact/Switch] Ditekan";
            _lastVisualOutput = isNightShowcaseMode ? "Mode Malam Pameran (Spotlight Fokus Dramatis)" : "Mode Siang Galeri (Pencahayaan Terang Penuh)";

            ApplyLightingMode();

            PlaySpatialSound(_switchClip, viewerTransform.position, 0.9f);
            _lastAudioOutput = "Audio Klik Saklar Lampu Galeri";
        }
    }

    /// <summary>
    /// Menerapkan perubahan tata cahaya galeri secara langsung ke ambient, lampu ruangan, dan spotlight maket.
    /// </summary>
    public void ApplyLightingMode()
    {
        if (galleryRoomLight != null)
        {
            galleryRoomLight.intensity = isNightShowcaseMode ? 0.05f : 1.4f;
        }

        if (galleryDirectionalLight != null)
        {
            galleryDirectionalLight.intensity = isNightShowcaseMode ? 0.05f : 0.9f;
        }

        if (exhibitionSpotlight != null)
        {
            exhibitionSpotlight.intensity = isNightShowcaseMode ? 4.2f : 1.6f;
            exhibitionSpotlight.color = isNightShowcaseMode ? new Color(1.0f, 0.90f, 0.72f) : new Color(0.95f, 0.98f, 1.0f);
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = isNightShowcaseMode ? new Color(0.04f, 0.04f, 0.06f) : new Color(0.55f, 0.55f, 0.58f);
    }

    /// <summary>
    /// Memicu umpan balik ganda untuk interaksi klik pada objek.
    /// </summary>
    private void TriggerInteractionFeedback(Transform target, Vector3 worldPoint, string objectName)
    {
        _lastActionName = $"Klik Kiri Mouse -> [{objectName}]";

        // (a) Kanal Output Visual (Pulsa Skala Halus & Terjamin Kembali ke Ukuran Asli)
        if (target != null)
        {
            if (!_baseScales.ContainsKey(target))
            {
                _baseScales[target] = target.localScale;
            }

            // Jika ada animasi pulsa yang masih berjalan, hentikan dan kembalikan ke skala awal seketika
            if (_activePulses.TryGetValue(target, out Coroutine existingPulse) && existingPulse != null)
            {
                StopCoroutine(existingPulse);
                target.localScale = _baseScales[target];
            }

            _activePulses[target] = StartCoroutine(PulseAnimation(target, _baseScales[target]));
        }

        if (infoBoardText != null)
        {
            infoBoardText.text = "<b><color=#FFD700><size=115%>GALERI PENGENALAN KAMPUS</size></color></b>\n" +
                                 "<size=80%><color=#8FA0B5>Sistem Informasi Pameran Virtual (Sub-CPMK 1)</color></size>\n" +
                                 "─────────────────────────────────────\n" +
                                 $"<b>Objek Terpilih:</b> <color=#00FF66>{objectName}</color>\n" +
                                 $"<b>Material:</b> WARNA_UTAMA (<color=#DDA0DD>{ProjectConstants.WARNA_UTAMA}</color>)\n" +
                                 $"<b>Status:</b> Terdeteksi & Terinspeksi Aktif\n\n" +
                                 "<b>PETUNJUK KENDALI INPUT–OUTPUT:</b>\n" +
                                 "• <b>[Klik Kiri]:</b> Interaksi & Deteksi Maket\n" +
                                 "• <b>[Spasi]:</b> Toggle Rotasi Showcase Pameran\n" +
                                 "• <b>[Tombol E]:</b> Saklar Pencahayaan Galeri\n" +
                                 "• <b>[W,A,S,D]:</b> Navigasi Pengunjung Virtual";
        }

        _lastVisualOutput = $"Animasi Pulsa Skala + Highlight Visual [{objectName}]";

        // (b) Kanal Output Audio
        PlaySpatialSound(_chimeClip, worldPoint, 1.0f);
        _lastAudioOutput = "Audio Chime Harmonis 3D Spasial";
    }

    /// <summary>
    /// Menangani umpan balik pergerakan langkah kaki (WASD Locomotion).
    /// </summary>
    private void HandleFootstep()
    {
        _lastActionName = "Navigasi Keyboard [WASD] Aktif";
        _lastVisualOutput = $"Posisi Viewer Bergeser di Galeri: {viewerTransform.position:F2}";
        PlaySpatialSound(_footstepClip, viewerTransform.position - Vector3.up * 1.5f, 0.6f);
        _lastAudioOutput = "Audio Langkah Kaki Spasial (Footstep)";
    }

    private void UpdateTurntableAnimation()
    {
        if (isTurntableActive && interactiveMaket != null)
        {
            interactiveMaket.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
        }
    }

    private IEnumerator PulseAnimation(Transform target, Vector3 baseScale)
    {
        Vector3 peakScale = baseScale * 1.08f; // Mengembang halus 8%

        // Mengembang
        float elapsed = 0f;
        while (elapsed < 0.08f)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(baseScale, peakScale, elapsed / 0.08f);
            yield return null;
        }

        // Mengempis kembali persis ke baseScale
        elapsed = 0f;
        while (elapsed < 0.10f)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(peakScale, baseScale, elapsed / 0.10f);
            yield return null;
        }

        // Jamin 100% kembali persis ke ukuran semula
        target.localScale = baseScale;
        _activePulses.Remove(target);
    }

    private void PlaySpatialSound(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        if (spatialAudioSource != null)
        {
            spatialAudioSource.transform.position = position;
            spatialAudioSource.pitch = Random.Range(0.95f, 1.05f);
            spatialAudioSource.PlayOneShot(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }

    /// <summary>
    /// Membaca sensor dan perangkat input secara aktual, lalu memformat teks pemantauan real-time.
    /// </summary>
    private void RenderMonitoringHUD()
    {
        _sb.Clear();
        _sb.AppendLine("<b><color=#FFD700>SISTEM MONITOR INPUT–OUTPUT</color></b>");
        _sb.AppendLine($"<size=82%>Proyek: {ProjectConstants.NIM} | Tema: {ProjectConstants.TEMA_APLIKASI} | Warna: {ProjectConstants.WARNA_UTAMA}</size>");
        _sb.AppendLine("<color=#445566>───────────────────────────────────────────</color>");

        // 1. DATA POSISI & ORIENTASI VIEWER (6-DoF)
        Vector3 vPos = viewerTransform != null ? viewerTransform.position : Vector3.zero;
        Vector3 vEuler = viewerTransform != null ? viewerTransform.eulerAngles : Vector3.zero;
        float pitch = vEuler.x > 180f ? vEuler.x - 360f : vEuler.x;
        float yaw = vEuler.y;
        float roll = vEuler.z > 180f ? vEuler.z - 360f : vEuler.z;

        _sb.AppendLine("<b>[VIEWER POSE - 6 DoF]</b>");
        _sb.AppendFormat("• Posisi : X:{0,5:F2} | Y:{1,5:F2} | Z:{2,5:F2} (m)\n", vPos.x, vPos.y, vPos.z);
        _sb.AppendFormat("• Rotasi : Yaw:{0,5:F1}° | Pitch:{1,5:F1}° | Roll:{2,5:F1}°\n", yaw, pitch, roll);
        _sb.AppendLine("<color=#445566>───────────────────────────────────────────</color>");

        // 2. STATUS PERANGKAT 1: KEYBOARD
        float axisH = Input.GetAxisRaw("Horizontal");
        float axisV = Input.GetAxisRaw("Vertical");
        bool keySpace = Input.GetKey(KeyCode.Space);
        bool keyE = Input.GetKey(KeyCode.E);
        bool keyShift = Input.GetKey(KeyCode.LeftShift);

        _sb.AppendLine("<b>[PERANGKAT INPUT 1: KEYBOARD]</b>");
        _sb.AppendFormat("• Sumbu WASD : H:{0,4:F1} | V:{1,4:F1}\n", axisH, axisV);
        _sb.AppendFormat("• Tombol Aksi: Space:[{0}] | E:[{1}] | Shift:[{2}]\n",
            keySpace ? "<color=#00FF66>ON</color>" : "OFF",
            keyE ? "<color=#00FF66>ON</color>" : "OFF",
            keyShift ? "<color=#00FF66>ON</color>" : "OFF");
        _sb.AppendLine("<color=#445566>───────────────────────────────────────────</color>");

        // 3. STATUS PERANGKAT 2: MOUSE
        Vector3 mousePos = Input.mousePosition;
        float mouseDeltaX = Input.GetAxis("Mouse X");
        float mouseDeltaY = Input.GetAxis("Mouse Y");
        float scrollDelta = Input.mouseScrollDelta.y;
        bool lmb = Input.GetMouseButton(0);
        bool rmb = Input.GetMouseButton(1);

        _sb.AppendLine("<b>[PERANGKAT INPUT 2: MOUSE]</b>");
        _sb.AppendFormat("• Posisi : ({0,4:F0}, {1,4:F0}) px | dX/dY: ({2,4:F1}, {3,4:F1})\n",
            mousePos.x, mousePos.y, mouseDeltaX, mouseDeltaY);
        _sb.AppendFormat("• Tombol : Klik Kiri:[{0}] | Klik Kanan:[{1}] | Scroll:{2:F1}\n",
            lmb ? "<color=#00FF66>DOWN</color>" : "UP",
            rmb ? "<color=#00FF66>HOLD</color>" : "UP",
            scrollDelta);
        _sb.AppendLine("<color=#445566>───────────────────────────────────────────</color>");

        // 4. STATUS PERANGKAT 3: GAMEPAD / VIRTUAL CONTROLLER
        ReadThirdDeviceStatus(_sb);
        _sb.AppendLine("<color=#445566>───────────────────────────────────────────</color>");

        // 5. STATUS RESPON GANDA KANAL OUTPUT
        _sb.AppendLine("<b>[RESPON JALUR KELUARAN (DUAL OUTPUT)]</b>");
        _sb.AppendLine($"• Input Terakhir : <color=#00FFFF>{_lastActionName}</color>");
        _sb.AppendLine($"• Visual  : {_lastVisualOutput}");
        _sb.AppendLine($"• Audio 3D: {_lastAudioOutput}");

        if (statusText != null)
        {
            statusText.text = _sb.ToString();
        }
    }

    /// <summary>
    /// Membaca perangkat ke-3 (Gamepad fisik atau Virtual Controller sekunder secara dinamis).
    /// </summary>
    private void ReadThirdDeviceStatus(StringBuilder sb)
    {
        bool gamepadFound = false;
        Vector2 stick = Vector2.zero;
        float trigger = 0f;
        bool buttonPressed = false;
        string deviceLabel = "Gamepad Fisik";

#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            gamepadFound = true;
            stick = pad.leftStick.ReadValue();
            trigger = pad.rightTrigger.ReadValue();
            buttonPressed = pad.buttonSouth.isPressed || pad.buttonWest.isPressed;
            deviceLabel = pad.displayName;
        }
#endif

        if (!gamepadFound)
        {
            // Fallback cerdas: Virtual Secondary Controller (membaca tombol panah dan modifier alt/ctrl)
            // agar selalu menghasilkan data real-time saat didemokan di lab tanpa colokan gamepad
            deviceLabel = "Virtual Controller (Keyboard Fallback)";
            float arrowX = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float arrowY = (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            stick = new Vector2(arrowX, arrowY);
            trigger = Input.GetKey(KeyCode.LeftControl) ? 1.0f : 0.0f;
            buttonPressed = Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
        }

        sb.AppendLine($"<b>[PERANGKAT INPUT 3: {deviceLabel.ToUpper()}]</b>");
        sb.AppendFormat("• Stick: ({0,4:F2}, {1,4:F2}) | Trigger: {2:F2} | Btn: [{3}]\n",
            stick.x, stick.y, trigger,
            buttonPressed ? "<color=#00FF66>PRESS</color>" : "IDLE");
    }

    /// <summary>
    /// Fallback IMGUI: Menjamin HUD tetap tampil di layar meskipun Canvas TextMeshPro belum di-assign.
    /// </summary>
    private void OnGUI()
    {
        if (statusText == null)
        {
            GUI.Box(new Rect(10, 10, 480, 420), "IOMonitor (Fallback HUD)");
            GUI.Label(new Rect(20, 35, 460, 390), _sb.ToString());
        }
    }
}
