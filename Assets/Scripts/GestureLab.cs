using UnityEngine;

public class GestureLab : MonoBehaviour
{
    [Header("XR References")]
    public Transform headCamera;
    public Transform rightController;

    [Header("Gesture Settings")]
    [Tooltip("Dapat diubah dari Inspector saat Play Mode (Nilai awal d6 = 0.9 m/s)")]
    public float AMBANG_GESTUR = 0.9f;
    public float cooldownTime = 0.5f;

    [Header("Status Output (Read-Only)")]
    public string lastDetectedGesture = "Belum Ada Gestur";
    public float currentSpeed = 0f;

    [Header("Camera Control Settings")]
    public float mouseSensitivity = 2.0f;

    // Camera Look internal variables
    private float yaw = 0f;
    private float pitch = 0f;

    private Vector3 previousControllerPosition;
    private bool initialized;
    private float lastGestureTime;

    // Impulse velocity simulation (prevents breaking controller Ray Visuals)
    private Vector3 simulatedLocalVelocity = Vector3.zero;
    private float simulatedVelocityDecayTime = 0f;

    void Start()
    {
        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;

        if (headCamera != null)
        {
            yaw = headCamera.localEulerAngles.y;
            pitch = headCamera.localEulerAngles.x;

            // Safely disable TrackedPoseDriver in Editor so it doesn't lock camera rotation
            Component poseDriver = headCamera.GetComponent("TrackedPoseDriver");
            if (poseDriver != null && poseDriver is Behaviour behaviour)
            {
                behaviour.enabled = false;
                Debug.Log("<color=yellow>[GestureLab] TrackedPoseDriver kamera dinonaktifkan sementara untuk navigasi Editor.</color>");
            }
        }

        if (rightController != null)
        {
            previousControllerPosition = rightController.position;
            initialized = true;
        }
    }

    void Update()
    {
        // -------------------------------------------------------------
        // 1. Camera Rotation (Mouse Right-Click + Drag & T Key 180° Turn)
        // -------------------------------------------------------------
        if (headCamera != null)
        {
            // Right-Click + Mouse Drag to look around freely
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity * 50f * Time.deltaTime;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 50f * Time.deltaTime;
                pitch = Mathf.Clamp(pitch, -80f, 80f);

                headCamera.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            }

            // Press 'T' to turn camera 180 degrees instantly (Sub-CPMK 5 proof requirement)
            if (Input.GetKeyDown(KeyCode.T))
            {
                yaw += 180f;
                headCamera.localRotation = Quaternion.Euler(pitch, yaw, 0f);
                Debug.Log("<color=yellow>[GestureLab] Kamera diputar 180 derajat!</color>");
            }
        }

        // -------------------------------------------------------------
        // 2. Gesture Shortcuts (P = Push, R = Raise Hand)
        // -------------------------------------------------------------
        // Press P: Forward impulse in local Z axis
        if (Input.GetKeyDown(KeyCode.P))
        {
            simulatedLocalVelocity = new Vector3(0f, 0f, AMBANG_GESTUR + 0.6f);
            simulatedVelocityDecayTime = Time.time + 0.25f;
        }

        // Press R: Upward impulse in local Y axis
        if (Input.GetKeyDown(KeyCode.R))
        {
            simulatedLocalVelocity = new Vector3(0f, AMBANG_GESTUR + 0.6f, 0f);
            simulatedVelocityDecayTime = Time.time + 0.25f;
        }

        // -------------------------------------------------------------
        // 3. Velocity Calculation & Gesture Detection
        // -------------------------------------------------------------
        Vector3 localVelocity = Vector3.zero;

        if (Time.time < simulatedVelocityDecayTime)
        {
            // Use impulse velocity from debug key press
            localVelocity = simulatedLocalVelocity;
        }
        else if (headCamera != null && rightController != null)
        {
            // Track physical controller movement relative to head
            Vector3 currentPosition = rightController.position;

            if (!initialized)
            {
                previousControllerPosition = currentPosition;
                initialized = true;
            }
            else
            {
                Vector3 worldVelocity = (currentPosition - previousControllerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
                previousControllerPosition = currentPosition;
                localVelocity = headCamera.InverseTransformDirection(worldVelocity);
            }
        }

        currentSpeed = localVelocity.magnitude;

        // Check if gesture speed exceeds threshold
        if (currentSpeed >= AMBANG_GESTUR && (Time.time - lastGestureTime) > cooldownTime)
        {
            // Push: Forward movement dominates (Z > Y)
            if (localVelocity.z > 0f && localVelocity.z > Mathf.Abs(localVelocity.y))
            {
                lastDetectedGesture = "Gestur Push Terdeteksi!";
                Debug.Log($"<color=green>{lastDetectedGesture}</color> (Kecepatan: {currentSpeed:F2} m/s)");
                lastGestureTime = Time.time;
            }
            // Raise Hand: Upward movement dominates (Y > Z)
            else if (localVelocity.y > 0f && localVelocity.y > Mathf.Abs(localVelocity.z))
            {
                lastDetectedGesture = "Gestur Raise Hand Terdeteksi!";
                Debug.Log($"<color=cyan>{lastDetectedGesture}</color> (Kecepatan: {currentSpeed:F2} m/s)");
                lastGestureTime = Time.time;
            }
        }
    }

    // -------------------------------------------------------------
    // 4. On-Screen Feedback UI
    // -------------------------------------------------------------
    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 350, 110), "Gesture Detection Monitor (Sub-CPMK 5)");
        GUI.Label(new Rect(20, 30, 330, 20), $"AMBANG_GESTUR: {AMBANG_GESTUR:F2} m/s");
        GUI.Label(new Rect(20, 50, 330, 20), $"Kecepatan Tangan: {currentSpeed:F2} m/s");
        GUI.Label(new Rect(20, 70, 330, 25), $"<b>Status: {lastDetectedGesture}</b>");
        GUI.Label(new Rect(20, 90, 330, 20), "<size=10>Klik Kanan + Drag: Putar Kamera | T: 180° | P: Push | R: Raise</size>");
    }
}
