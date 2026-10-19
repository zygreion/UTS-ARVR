using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public enum RotationType { None, Direction, Mouse };

    [Header("=== KEYS ===")]
    public KeyCode forward = KeyCode.W;
    public KeyCode left = KeyCode.A;
    public KeyCode back = KeyCode.S;
    public KeyCode right = KeyCode.D;
    public KeyCode up = KeyCode.E;
    public KeyCode down = KeyCode.Q;

    [Header("=== ATTRIBUTES ===")]
    public KeyCode slow = KeyCode.LeftShift;
    public float moveSpeed = 5f;
    public bool slowMotion = false;
    public bool autoMove = true;

    [Header("=== ROTATION ===")]
    public RotationType rotationType = RotationType.None;
    [Min(0)] public float mouseSensitivity = 2.5f;

    [Header("=== GIZMOS ===")]
    public bool drawRay = true;
    [Range(0f, 1f)] public float rayLength = 0.5f;

    // ----- Internal Variables -----
    private float _pitch;
    private float _yaw;
    private Vector3 _direction;
    public Vector3 _targetPos;
    public Vector3 _previousPos;

    public Action BeforeMove;

    void Start()
    {
        _targetPos = transform.position;
    }

    void Update()
    {
        Rotate();
        UpdateTargetPos();

        if (autoMove)
        {
            BeforeMove?.Invoke();
            MoveToTarget();
        }
    }

    protected virtual void UpdateTargetPos()
    {
        Vector3 newDirection = Vector3.zero;

        Vector3 f = rotationType == RotationType.Mouse ? Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized : Vector3.forward;
        Vector3 r = rotationType == RotationType.Mouse ? Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized : Vector3.right;

        if (Input.GetKey(forward)) newDirection += f;
        if (Input.GetKey(back)) newDirection -= f;
        if (Input.GetKey(right)) newDirection += r;
        if (Input.GetKey(left)) newDirection -= r;
        if (Input.GetKey(up)) newDirection += Vector3.up;
        if (Input.GetKey(down)) newDirection -= Vector3.up;

        newDirection.Normalize();

        // Moving from inspector is allowed now if user doesn't press any key
        if (newDirection == Vector3.zero)
        {
            // targetPos = transform.position;
            return;
        }

        // Left Shift will make movement 60% slower
        slowMotion = Input.GetKey(slow);
        float newSpeed = moveSpeed * (slowMotion ? 0.4f : 1f);

        _targetPos += newSpeed * Time.deltaTime * newDirection;
        _direction = newDirection;
    }

    public virtual void OverrideNextPos(Vector3 pos) => _targetPos = pos;

    public virtual void MoveToTarget()
    {
        transform.position = _targetPos;
        _previousPos = _targetPos;
    }

    protected virtual void Rotate()
    {
        if (rotationType == RotationType.None || _direction == Vector3.zero) return;
        if (rotationType == RotationType.Direction) transform.rotation = Quaternion.LookRotation(_direction);
        // Below will always be RotationType.Mouse
        if (rotationType == RotationType.Mouse && Input.GetMouseButton(1))
        {
            _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -80f, 80f);
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (!drawRay) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * rayLength);
    }
}
