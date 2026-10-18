using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LocomotionSwitcher : MonoBehaviour
{
    [Header("Locomotion Providers")]
    public TeleportationProvider teleportationProvider;
    public ContinuousMoveProviderBase continuousMoveProvider;
    public SnapTurnProviderBase snapTurnProvider;

    [Header("Editor Smooth Locomotion Settings")]
    public float moveSpeed = 2.5f;

    [Header("Status (Play Mode)")]
    public bool isTeleportActive = true;

    private Transform rigTransform;

    void Start()
    {
        rigTransform = transform;
        UpdateLocomotionMode();
    }

    void Update()
    {
        // Tekan tombol 'L' di Editor untuk menukar mode Locomotion
        if (Input.GetKeyDown(KeyCode.L))
        {
            isTeleportActive = !isTeleportActive;
            UpdateLocomotionMode();
            Debug.Log("<color=yellow>Mode Locomotion diubah ke: " + (isTeleportActive ? "Teleportation" : "Smooth Locomotion") + "</color>");
        }

        // Jika dalam mode Smooth Locomotion, izinkan pergerakan WASD di Editor
        if (!isTeleportActive)
        {
            HandleEditorWASDMovement();
        }
    }

    private void HandleEditorWASDMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical");   // W/S

        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f)
        {
            Transform cam = Camera.main != null ? Camera.main.transform : rigTransform;
            
            // Ambil arah depan & kanan sejajar lantai (X/Z plane)
            Vector3 forward = cam.forward;
            Vector3 right = cam.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * moveZ + right * moveX).normalized;
            rigTransform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    public void UpdateLocomotionMode()
    {
        if (teleportationProvider != null) 
            teleportationProvider.enabled = isTeleportActive;

        if (continuousMoveProvider != null) 
            continuousMoveProvider.enabled = !isTeleportActive;

        if (snapTurnProvider != null) 
            snapTurnProvider.enabled = true; // Fitur kenyamanan Snap Turn
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10, 120, 320, 60), "Locomotion System (Sub-CPMK 5c)");
        GUI.Label(new Rect(20, 140, 300, 20), $"Mode Aktif (Tekan L): <b>{(isTeleportActive ? "Teleportation" : "Smooth Locomotion")}</b>");
        if (!isTeleportActive)
        {
            GUI.Label(new Rect(20, 158, 300, 20), "<color=cyan>Gunakan WASD untuk berjalan di Editor</color>");
        }
    }
}
