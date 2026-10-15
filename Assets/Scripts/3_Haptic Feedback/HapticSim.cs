#define PAKAI_INPUT_SYSTEM

using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR;
using static ProjectConstants;

#if PAKAI_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Input System dan XR sama-sama memiliki kelas bernama InputDevice, sehingga
// nama tersebut menjadi ambigu bila keduanya diimpor. Alias di bawah ini
// menegaskan bahwa yang dimaksud adalah InputDevice milik XR.
using XRInputDevice = UnityEngine.XR.InputDevice;

public class HapticSim : MonoBehaviour
{
    [Header("=== LAPIS SIMULASI (perangkat lunak) ===")]
    [Tooltip("Kekakuan bahan dalam newton per meter. Nilai besar = terasa keras.")]
    [Range(MIN_KEKAKUAN, MAX_KEKAKUAN)] public float kekakuan = KEKAKUAN_BAHAN;

    [Tooltip("Redaman, melawan kecepatan penetrasi. Mencegah osilasi.")]
    [Range(MIN_REDAMAN, MAX_REDAMAN)] public float redaman = 20f;

    [Tooltip("Gaya maksimum yang boleh dihasilkan, dalam newton.")]
    [Range(MIN_KEKAKUAN + 0.1f, MAX_KEKAKUAN)] public float gayaMaksimum = 10f;

    [Header("=== LAPIS PERANGKAT (perangkat keras) ===")]
    [Tooltip("Amplitudo minimum agar getaran masih terasa (0..1).")]
    [Range(0f, 0.5f)] public float _amplitudoMinimum = 0.05f;

    [Header("=== PSEUDO-HAPTICS ===")]
    [Tooltip("Memperlambat gerak objek sebanding dengan gaya. Tidak perlu device XR asli.")]
    public bool pseudoHaptics = true;

    [Tooltip("Memperlambat gerak objek sebanding dengan gaya. Tidak perlu device XR asli.")]
    [Range(0f, 1f)] public float pseudoPower = 0.7f;

    [Header("=== GERAK ===")]
    [Tooltip("Kecepatan bergerak.")]
    [Range(0f, 20f)] public float moveSpeed = 5f;

    public bool slowMotion = false;

    // ----- Variabel internal -----
    private Collider _hitCollider;
    private string _namaBahan = "-";
    private float _gaya;
    private float _gayaNoCapped;
    private float _amplitudo;  // 0...1
    Vector3 _targetPos;
    private float _penetrasi;  // (dalam meter)
    private float _penetrasiSebelumnya;  // (dalam meter)
    private HapticInfoUI hapticInfoUI;
    private Collider _collider;
    private Vector3 _normalKeluar;

    // Start is called before the first frame update
    void Start()
    {
        _targetPos = transform.position;

        hapticInfoUI = gameObject.GetComponent<HapticInfoUI>();
        _collider = gameObject.GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        _gaya = HitungGaya();

        _amplitudo = Mathf.Clamp01(_gaya / gayaMaksimum);
        if (_amplitudo > 0f) _amplitudo = Mathf.Max(_amplitudo, _amplitudoMinimum);
        KirimHaptik(_amplitudo);

        if (hapticInfoUI != null)
        {
            hapticInfoUI.UpdateInfoUI(
                _namaBahan, kekakuan, _penetrasi,
                _hitCollider ? _gayaNoCapped : 0f, _hitCollider ? _gaya : 0f, gayaMaksimum,
                _amplitudo, pseudoHaptics);
        }
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
        Vector3 _direction = Vector3.zero;
        foreach (var item in key2MoveVector)
        {
            if (Input.GetKey(item.Key)) _direction += item.Value;
        }

        // Moving from inspector is allowed now if user doesn't press any key
        if (_direction == Vector3.zero)
        {
            _targetPos = transform.position;
            return;
        }
        _direction.Normalize();

        // Go on if user press any key

        // Left Shift will make movement 60% slower
        slowMotion = Input.GetKey(KeyCode.LeftShift);
        float newSpeed = moveSpeed * (slowMotion ? 0.4f : 1f);

        _targetPos += newSpeed * Time.deltaTime * _direction;

        // When user is moving object from inspector, the _targetPos is equal to transform.position
        // So lerping position will become irrelevant here
        if (pseudoHaptics)
        {
            // Hambatan hanya berlaku saat MENEKAN lebih dalam (arah gerak melawan normal keluar).
            // Saat menarik keluar tidak dihambat, supaya terasa lepas, bukan lengket.
            bool menekanMasuk = _hitCollider != null && Vector3.Dot(_direction, _normalKeluar) < 0f;
            float resistance = 1f;

            if (menekanMasuk)
            {
                resistance = 1f - (Mathf.Clamp01(_gaya / gayaMaksimum) * pseudoPower);
                resistance = Mathf.Max(resistance, 0.02f);
            }

            transform.position = Vector3.Lerp(transform.position, _targetPos, resistance);
        }
        else transform.position = _targetPos;

        transform.rotation = Quaternion.LookRotation(_direction);
    }

    float HitungPenetrasi(out Vector3 arahKeluar)
    {
        arahKeluar = Vector3.zero;
        if (_collider == null || _hitCollider == null) return 0f;

        if (Physics.ComputePenetration(
            _collider, transform.position, transform.rotation,
            _hitCollider, _hitCollider.transform.position, _hitCollider.transform.rotation,
            out arahKeluar, out float jarak))
            return jarak;
        return 0f;
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

        // Vector3 closestPoint = _hitCollider.ClosestPoint(transform.position);
        // _penetrasi = Vector3.Distance(transform.position, closestPoint);
        _penetrasi = HitungPenetrasi(out _normalKeluar);

        // if (_penetrasi < float.Epsilon)
        // {
        //     _penetrasi = Vector3.Distance(transform.position, _hitCollider.ClosestPointOnBounds(transform.position)) + 0.01f;
        // }

        // Inilah dd / dt
        // dd = perbedaan jarak penetrasi
        // dt = perbedaan waktu. menggunakan max float.Epsilon agar pembagi tidak menjadi 0 yang akan mengakibatkan error
        float kecepatanPenetrasi = (_penetrasi - _penetrasiSebelumnya) / Mathf.Max(Time.deltaTime, float.Epsilon);
        _penetrasiSebelumnya = _penetrasi;

        // Inilah F = (k x d) + (b x dd/dt)
        // k = kekakuan
        // d = penetrasi
        // b = redaman
        float F = (kekakuan * _penetrasi) + (redaman * Mathf.Max(kecepatanPenetrasi, 0f));
        _gayaNoCapped = F;
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
            _namaBahan = materialHaptik.namaBahan;
            kekakuan = materialHaptik.kekakuan;
            redaman = materialHaptik.redaman;
        }
        else _namaBahan = other.name;
    }

    void OnTriggerExit(Collider other)
    {
        if (_hitCollider.Equals(other))
        {
            _hitCollider = null;
            _namaBahan = "-";
        }
    }

    void OnDisable()
    {
#if PAKAI_INPUT_SYSTEM
        // if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0f, 0f);
#endif
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.075f);
        Gizmos.DrawRay(transform.position, transform.forward * 0.2f);
    }
}
