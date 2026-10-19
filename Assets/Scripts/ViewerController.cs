using System;
using UnityEngine;

/// <summary>
/// ViewerController: Mengendalikan navigasi kamera viewer secara first-person.
/// Mendukung pergerakan translasi WASD dan rotasi sudut pandang kamera (Mouse Look).
/// </summary>
[DisallowMultipleComponent]
public class ViewerController : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    [Tooltip("Kecepatan jalan normal (meter/detik).")]
    public float walkSpeed = 3.5f;

    [Tooltip("Kecepatan lari dengan tombol Shift (meter/detik).")]
    public float sprintSpeed = 6.0f;

    [Tooltip("Batas area jelajah di dalam ruangan (meter).")]
    public Vector2 boundaryMin = new Vector2(-6.0f, -6.0f);
    public Vector2 boundaryMax = new Vector2(6.0f, 6.0f);

    [Header("Pengaturan Pandangan (Mouse Look)")]
    [Tooltip("Sensitivitas rotasi mouse.")]
    public float mouseSensitivity = 2.5f;

    [Tooltip("Batasan sudut vertikal (pitch) dalam derajat.")]
    public float minPitch = -75.0f;
    public float maxPitch = 80.0f;

    [Tooltip("Tahan klik kanan untuk memutar sudut pandang (false = rotasi langsung).")]
    public bool requireRightClickToLook = true;

    // Status internal pergerakan
    public Vector3 CurrentVelocity { get; private set; }
    public bool IsMoving => CurrentVelocity.sqrMagnitude > 0.05f;

    // Event langkah kaki untuk memicu output audio pergerakan
    public event Action OnStepTaken;

    private float _yaw;
    private float _pitch;
    private float _distanceCovered;
    private const float StepInterval = 1.6f; // Tiap 1.6 meter memicu suara langkah

    private void Start()
    {
        Vector3 initialEuler = transform.eulerAngles;
        _yaw = initialEuler.y;
        _pitch = initialEuler.x;

        // Normalisasi pitch jika berada di kuadran atas
        if (_pitch > 180f) _pitch -= 360f;
    }

    private void Update()
    {
        HandleRotation();
        HandleTranslation();
    }

    private void HandleRotation()
    {
        bool canRotate = !requireRightClickToLook || Input.GetMouseButton(1);

        if (canRotate)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 40f * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 40f * Time.deltaTime;

            _yaw += mouseX;
            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }

    private void HandleTranslation()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Proyeksi ke bidang datar (horizontal XZ)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = (forward * v + right * h).normalized;
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        CurrentVelocity = direction * speed;
        Vector3 displacement = CurrentVelocity * Time.deltaTime;

        Vector3 targetPos = transform.position + displacement;

        // Batasi di dalam perimeter ruangan galeri
        targetPos.x = Mathf.Clamp(targetPos.x, boundaryMin.x, boundaryMax.x);
        targetPos.z = Mathf.Clamp(targetPos.z, boundaryMin.y, boundaryMax.y);

        transform.position = targetPos;

        // Deteksi akumulasi langkah kaki
        if (displacement.magnitude > 0.001f)
        {
            _distanceCovered += displacement.magnitude;
            if (_distanceCovered >= StepInterval)
            {
                _distanceCovered = 0f;
                OnStepTaken?.Invoke();
            }
        }
    }
}
