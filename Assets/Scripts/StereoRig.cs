
using UnityEngine;

[DisallowMultipleComponent]
public class StereoRig : MonoBehaviour
{
    public enum StereoMode
    {
        OffAxis,
        ToeIn
    }

    [Header("Camera References")]
    [SerializeField] private Camera leftCamera;
    [SerializeField] private Camera rightCamera;

    [Header("Parameter Berdasarkan NIM")]
    [SerializeField, Min(1f)]
    private float ipdMillimeters = 68f;

    [SerializeField, Min(0.1f)]
    private float convergenceDistance = 3f;

    [Header("Runtime Controls")]
    [SerializeField]
    private StereoMode currentMode = StereoMode.OffAxis;

    [SerializeField] private KeyCode offAxisKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode toeInKey = KeyCode.Alpha2;

    private Vector3 leftStartPosition;
    private Vector3 rightStartPosition;
    private Quaternion leftStartRotation;
    private Quaternion rightStartRotation;
    private bool initialized;

    private void Awake()
    {
        if (leftCamera == null || rightCamera == null)
        {
            Debug.LogError(
                "StereoRig: Assign the left and right cameras in the Inspector."
            );

            enabled = false;
            return;
        }

        if (leftCamera == rightCamera)
        {
            Debug.LogError(
                "StereoRig: The left and right cameras must be different."
            );

            enabled = false;
            return;
        }

        if (leftCamera.transform.parent != transform ||
            rightCamera.transform.parent != transform)
        {
            Debug.LogError(
                "StereoRig: Both cameras must be direct children " +
                "of StereoRigController."
            );

            enabled = false;
            return;
        }

        leftStartPosition = leftCamera.transform.localPosition;
        rightStartPosition = rightCamera.transform.localPosition;

        leftStartRotation = leftCamera.transform.localRotation;
        rightStartRotation = rightCamera.transform.localRotation;

        initialized = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(offAxisKey))
        {
            currentMode = StereoMode.OffAxis;
            Debug.Log("Stereo mode changed to Off-Axis.");
        }

        if (Input.GetKeyDown(toeInKey))
        {
            currentMode = StereoMode.ToeIn;
            Debug.Log("Stereo mode changed to Toe-In.");
        }
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        float halfIPD = ipdMillimeters / 2000f;

        leftCamera.transform.localPosition = new Vector3(
            -halfIPD,
            leftStartPosition.y,
            leftStartPosition.z
        );

        rightCamera.transform.localPosition = new Vector3(
            halfIPD,
            rightStartPosition.y,
            rightStartPosition.z
        );

        UpdateCameraAspect(leftCamera);
        UpdateCameraAspect(rightCamera);

        if (currentMode == StereoMode.OffAxis)
        {
            ApplyOffAxis();
        }
        else
        {
            ApplyToeIn();
        }
    }

    private void UpdateCameraAspect(Camera cam)
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        if (cam.rect.width <= 0f || cam.rect.height <= 0f)
            return;

        float viewportWidth = Screen.width * cam.rect.width;
        float viewportHeight = Screen.height * cam.rect.height;

        cam.aspect = viewportWidth / viewportHeight;
    }

    private void ApplyOffAxis()
    {
        //Keep both cameras parallel.
        leftCamera.transform.localRotation = Quaternion.identity;
        rightCamera.transform.localRotation = Quaternion.identity;

        ApplyOffAxisFrustum(leftCamera);
        ApplyOffAxisFrustum(rightCamera);
    }

    private void ApplyOffAxisFrustum(Camera cam)
    {
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        float halfHeight = near * Mathf.Tan(
            cam.fieldOfView * 0.5f * Mathf.Deg2Rad
        );

        float halfWidth = halfHeight * cam.aspect;

        float eyeOffset = cam.transform.localPosition.x;

        float shift = -eyeOffset * near / convergenceDistance;

        float left = -halfWidth + shift;
        float right = halfWidth + shift;
        float bottom = -halfHeight;
        float top = halfHeight;

        cam.projectionMatrix = Matrix4x4.Frustum(
            left,
            right,
            bottom,
            top,
            near,
            far
        );
    }

    private void ApplyToeIn()
    {
        leftCamera.ResetProjectionMatrix();
        rightCamera.ResetProjectionMatrix();

        //Find the midpoint between the two camera positions.
        Vector3 eyeMidpoint =
            (leftCamera.transform.position
            + rightCamera.transform.position) * 0.5f;

        //Convergence point stays at eye height.
        Vector3 target =
            eyeMidpoint + transform.forward * convergenceDistance;

        leftCamera.transform.rotation = Quaternion.LookRotation(
            target - leftCamera.transform.position,
            transform.up
        );

        rightCamera.transform.rotation = Quaternion.LookRotation(
            target - rightCamera.transform.position,
            transform.up
        );
    }

    private void OnGUI()
    {
        if (!initialized)
            return;

        GUI.Label(
            new Rect(10, 10, 400, 25),
            "Stereo: " + currentMode +
            " | 1 = Off-Axis | 2 = Toe-In"
        );
    }

    private void OnDisable()
    {
        if (leftCamera != null)
        {
            leftCamera.ResetProjectionMatrix();

            if (leftCamera.transform.parent == transform)
                leftCamera.transform.localRotation = leftStartRotation;
        }

        if (rightCamera != null)
        {
            rightCamera.ResetProjectionMatrix();

            if (rightCamera.transform.parent == transform)
                rightCamera.transform.localRotation = rightStartRotation;
        }
    }
}