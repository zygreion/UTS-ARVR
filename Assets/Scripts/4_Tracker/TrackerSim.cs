using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TrackerSim : MonoBehaviour
{
    public enum HiddenType { Hold, Hide }

    [Header("=== SUMBER DAN KELUARAN ===")]
    public Transform realObj;
    public Transform trackedObj;
    public Transform referenceViewer;

    [Header("=== ATTRIBUTES ===")]
    public string label = "Real Object";
    [Range(1f, 240f)] public float refreshHz = 90;
    [Range(0f, 1000f)] public float latencyMs = 100;
    [Range(0f, 100f)] public float driftMmPerSec;
    public Vector3 offsetKalibrasi;

    [Header("=== NOISE ===")]
    [Range(0f, 50f)] public float noisePosMm = ProjectConstants.NOISE_TRACKER;
    [Range(0f, 360f)] public float noiseRotDeg = ProjectConstants.NOISE_TRACKER;
    [Range(0f, 0.99f)] public float smoothing = 0f;
    public bool enableNoise = false;

    [Header("=== SENSOR OPTIK ===")]
    public Transform opticSensor;
    public HiddenType hiddenType = HiddenType.Hold;

    [Header("=== UI ===")]
    public TextMeshProUGUI textUI;

    // ----- Internal Variables -----
    private Vector3 _drift;
    private Vector3 _posRaw;
    private Quaternion _rotRaw;
    private Vector3 _posFilter;
    private Quaternion _rotFilter;
    private bool _hidden;
    private Renderer[] _renderers;

    struct Sample { public float t; public Vector3 p; public Quaternion r; };
    private float _now;
    private float _nextSampleTime;
    private Queue<Sample> _samples = new Queue<Sample>();
    private bool _ready;
    private float _error;
    private float _errorAvg;
    private Camera _refCamera;

    // Start is called before the first frame update
    void Start()
    {
        if (trackedObj != null)
            _renderers = trackedObj.GetComponentsInChildren<Renderer>();

        if (referenceViewer != null)
            _refCamera = referenceViewer.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (realObj == null && trackedObj == null) return;
        _now = Time.time;

        // 1. Drift
        Drift();

        // 2. Oklusi
        _hidden = CheckOcclusion();
        HideTracked(_hidden);

        // 3. Sample
        CreateSample();

        // 4. Latensi
        if (!LatencyReady()) return;

        // 5. Filter: exponential smoothing, dibuat tidak bergantung frame rate
        NoiseFilter();

        // 6. Galat Error
        CalulateError();

        trackedObj.SetPositionAndRotation(_posFilter, _rotFilter);
        UpdateInfoUI();

        if (_refCamera != null)
            _refCamera.enabled = Input.GetKey(KeyCode.Space);
    }

    // ----- Main Functions -----
    void Drift()
    {
        if (Input.GetKeyDown(KeyCode.R)) _drift = Vector3.zero;

        _drift += (driftMmPerSec / 1000f) * Time.deltaTime * Random.onUnitSphere;
    }

    bool CheckOcclusion()
    {
        if (opticSensor == null) return false;

        Vector3 source = opticSensor.transform.position;
        Vector3 direction = realObj.transform.position - source;

        RaycastHit[] raycastHits = Physics.RaycastAll(source, direction.normalized, direction.magnitude);
        foreach (var rh in raycastHits)
        {
            if (rh.transform == realObj || rh.transform.IsChildOf(realObj)) continue;
            if (trackedObj != null && (rh.transform == trackedObj || rh.transform.IsChildOf(trackedObj))) continue;
            return true;
        }
        return false;
    }

    void HideTracked(bool isHidden)
    {
        if (_renderers == null || hiddenType != HiddenType.Hide) return;

        foreach (var rd in _renderers)
        {
            rd.enabled = !isHidden;
        }
    }

    void CreateSample()
    {
        if (_now >= _nextSampleTime)
        {
            _nextSampleTime = _now + 1f / refreshHz;

            if (!_hidden)
            {
                Vector3 p = realObj.position
                            + (enableNoise ? 1f : 0f) * (noisePosMm / 1000f) * Random.onUnitSphere
                            + _drift
                            + offsetKalibrasi;

                Quaternion r = realObj.rotation
                              * Quaternion.Euler((enableNoise ? 1f : 0f) * noiseRotDeg * Random.onUnitSphere);

                _samples.Enqueue(new Sample { t = _now, p = p, r = r });
            }
        }
    }

    bool LatencyReady()
    {
        float limit = _now - latencyMs / 1000f;
        while (_samples.Count > 0 && _samples.Peek().t <= limit)
        {
            Sample s = _samples.Dequeue();
            _posRaw = s.p;
            _rotRaw = s.r;

            if (!_ready)
            {
                _posFilter = _posRaw;
                _rotFilter = _rotRaw;
                _ready = true;
            }
        }

        return _ready;
    }

    void NoiseFilter()
    {
        float alpha = 1f - Mathf.Pow(smoothing, Time.deltaTime * 60f);
        _posFilter = Vector3.Lerp(_posFilter, _posRaw, alpha);
        _rotFilter = Quaternion.Slerp(_rotFilter, _rotRaw, alpha);
    }

    void CalulateError()
    {
        _error = Vector3.Distance(trackedObj.position, realObj.position) * 1000f;
        _errorAvg = Mathf.Lerp(_errorAvg, _error, Time.deltaTime * 2f);
    }

    // ----- Helpers -----
    void OnDrawGizmos()
    {
        if (opticSensor != null && realObj != null)
        {
            Gizmos.color = _hidden ? Color.red : Color.green;
            Gizmos.DrawLine(opticSensor.position, realObj.position);
        }
        if (realObj != null && trackedObj != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(realObj.position, trackedObj.position);
        }
    }

    void UpdateInfoUI()
    {
        if (textUI == null) return;

        string status = _hidden ? "<color=#ff6b6b>TRACKING HILANG</color>" : "<color=#00d1b2>terlacak</color>";

        string rel = "";
        if (referenceViewer != null && trackedObj != null)
        {
            Vector3 lokal = referenceViewer.InverseTransformPoint(trackedObj.position);
            rel = $"\nRelatif thd viewer : ({lokal.x:F2}, {lokal.y:F2}, {lokal.z:F2}) m";
        }

        string statusText =
            $"<b>{label}</b>   [{status}]\n" +
            $"Laju {refreshHz:F0} Hz  |  Latensi {latencyMs:F0} ms  |  Noise {noisePosMm:F1} mm  |  Drift {driftMmPerSec:F1} mm/s\n" +
            $"Filter {smoothing:F2}  |  Galat saat ini {_error:F1} mm  |  Rata-rata {_errorAvg:F1} mm" + rel;

        textUI.text = statusText;
    }
}
