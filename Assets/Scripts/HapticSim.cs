#define PAKAI_INPUT_SYSTEM

using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR;

#if PAKAI_INPUT_SYSTEM
// using UnityEngine.InputSystem;
#endif

// Input System dan XR sama-sama memiliki kelas bernama InputDevice, sehingga
// nama tersebut menjadi ambigu bila keduanya diimpor. Alias di bawah ini
// menegaskan bahwa yang dimaksud adalah InputDevice milik XR.
using XRInputDevice = UnityEngine.XR.InputDevice;

public class HapticSim : MonoBehaviour
{
    [Header("=== LAPIS SIMULASI (perangkat lunak) ===")]
    [Tooltip("Default Kekakuan bahan dalam newton per meter. Nilai besar = terasa keras.")]
    [Range(10f, 2000f)] public float kekakuanDefault = ProjectConstants.KEKAKUAN_BAHAN;

    [Tooltip("Default Redaman, melawan kecepatan penetrasi. Mencegah osilasi.")]
    [Range(10f, 2000f)] public float redamanDefault = 20f;
    [Tooltip("Gaya maksimum yang boleh dihasilkan, dalam newton.")]
    [Range(5f, 60f)] public float gayaMaksimum = 15f;

    [Header("=== LAPIS PERANGKAT (perangkat keras) ===")]
    [Tooltip("Amplitudo minimum agar getaran masih terasa (0..1).")]
    [Range(0f, 0.5f)] public float _amplitudoMinimum = 0.05f;

    [Header("=== PSEUDO-HAPTICS ===")]
    [Tooltip("Memperlambat gerak objek sebanding dengan gaya. Tidak perlu device XR asli.")]
    public bool pseudoHaptics = true;

    [Tooltip("Memperlambat gerak objek sebanding dengan gaya. Tidak perlu device XR asli.")]
    [Range(0f, 1f)] public float pseudoPower = 0.7f;

    [Tooltip("Kecepatan bergerak.")]
    [Range(0f, 20f)] public float moveSpeed = 5f;

    public bool slowMotion = false;

    // ----- Variabel internal -----
    private Collider _hitCollider;
    private float _kekakuan;
    private float _redaman;
    private string _namaBahan;
    private float _gaya;
    private float _amplitudo;
    Vector3 _targetPos;
    private float _penetrasi;
    private float _penetrasiSebelumnya;

    // Start is called before the first frame update
    void Start()
    {
        _targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        _gaya = HitungGaya();

        _amplitudo = Mathf.Clamp01(_gaya / gayaMaksimum);
        if (_amplitudo > 0f) _amplitudo = Mathf.Max(_amplitudo, _amplitudoMinimum);
        KirimHaptik(_amplitudo);
    }

    readonly Dictionary<KeyCode, Vector3> key2MoveVector = new()
    {
        { KeyCode.W, Vector3.forward },
        { KeyCode.S, Vector3.back },
        { KeyCode.A, Vector3.left },
        { KeyCode.D, Vector3.right },
        { KeyCode.E, Vector3.up },
        { KeyCode.Q, Vector3.down },
    };

    void Move()
    {
        Vector3 direction = Vector3.zero;
        foreach (var item in key2MoveVector)
        {
            if (Input.GetKey(item.Key)) direction += item.Value;
        }

        slowMotion = Input.GetKey(KeyCode.LeftShift);
        float newSpeed = moveSpeed * (slowMotion ? 0.5f : 1f);

        _targetPos += newSpeed * Time.deltaTime * direction.normalized;

        if (pseudoHaptics)
        {
            float resistance = 1f - (Mathf.Clamp01(_gaya / gayaMaksimum) * pseudoPower);
            transform.position = Vector3.Lerp(transform.position, _targetPos, Mathf.Max(resistance, 0.02f));
        }
        else transform.position = _targetPos;
    }

    /// <summary>
    /// LAPIS SIMULASI — model pegas teredam.
    /// F = k x d + b x (dd/dt), dibatasi oleh gaya maksimum perangkat.
    /// </summary>
    float HitungGaya()
    {
        if (_hitCollider == null)
        {
            _penetrasi = 0f;
            _penetrasiSebelumnya = 0f;
            return 0f;
        }

        Vector3 closestPoint = _hitCollider.ClosestPoint(transform.position);
        _penetrasi = Vector3.Distance(transform.position, closestPoint);

        if (_penetrasi < 0.0001f)
        {
            _penetrasi = Vector3.Distance(transform.position, _hitCollider.ClosestPointOnBounds(transform.position)) + 0.01f;
        }

        // Inilah dd / dt
        // dd = perbedaan jarak penetrasi
        // dt = perbedaan waktu. menggunakan max float.Epsilon agar pembagi tidak menjadi 0 yang akan mengakibatkan error
        float kecepatanPenetrasi = (_penetrasi - _penetrasiSebelumnya) / Mathf.Max(Time.deltaTime, float.Epsilon);
        _penetrasiSebelumnya = _penetrasi;

        // Inilah F = (k x d) + (b x dd/dt)
        // k = kekakuan
        // d = penetrasi
        // b = redaman
        float F = (_kekakuan * _penetrasi) + (_redaman * Mathf.Max(kecepatanPenetrasi, 0f));
        return Mathf.Clamp(F, 0f, gayaMaksimum);
    }

    /// <summary>
    /// LAPIS PERANGKAT — mengirim amplitudo ke perangkat keras yang tersedia.
    /// </summary>
    void KirimHaptik(float amplitudo)
    {

    }


    // ----- Deteksi Kontak -----
    void OnTriggerEnter(Collider other)
    {
        _hitCollider = other;

        if (other.TryGetComponent<MaterialHaptik>(out var materialHaptik))
        {
            _kekakuan = materialHaptik.kekakuan;
            _redaman = materialHaptik.redaman;
            _namaBahan = materialHaptik.namaBahan;
        }
        else
        {
            _namaBahan = other.name;
            _kekakuan = kekakuanDefault;
            _redaman = redamanDefault;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_hitCollider.Equals(other))
        {
            _hitCollider = null;
            _namaBahan = "-";
        }
    }

    //     void OnDisable()
    //     {
    // #if PAKAI_INPUT_SYSTEM
    //         if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0f, 0f);
    // #endif
    //     }

    // ---------- Tampilan data di layar ----------
    void OnGUI()
    {
        GUI.color = Color.white;
        var gayaGUI = new GUIStyle(GUI.skin.label) { fontSize = 16, };

        GUI.Label(new Rect(14, 10, 900, 24), "WASD/QE = gerakkan probe   |   Tabrakkan objek untuk merasakan gaya", gayaGUI);
        GUI.Label(new Rect(14, 38, 900, 24), $"Bahan        : {_namaBahan}   (k = {(_hitCollider ? _kekakuan : 0f):F0} N/m)", gayaGUI);
        GUI.Label(new Rect(14, 62, 900, 24), $"Penetrasi    : {_penetrasi * 100f:F2} cm", gayaGUI);
        GUI.Label(new Rect(14, 86, 900, 24), $"Gaya simulasi: {_gaya:F2} N   (maks {gayaMaksimum:F0} N)", gayaGUI);
        GUI.Label(new Rect(14, 110, 900, 24), $"Amplitudo    : {_amplitudo:F2}", gayaGUI);
        GUI.Label(new Rect(14, 134, 900, 24), $"Pseudo-haptics: {(pseudoHaptics ? "aktif" : "nonaktif")}", gayaGUI);

        // Bar gaya
        GUI.Box(new Rect(14, 164, 300, 18), "");
        GUI.color = Color.Lerp(Color.green, Color.red, _amplitudo);
        GUI.Box(new Rect(14, 164, 300 * _amplitudo, 18), "");
    }
}
