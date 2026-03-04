using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerEggController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float turnSpeed = 1f;

    [Header("Camera (Cinemachine Target)")]
    public Transform cinemachineCameraTarget;
    public float lookSensitivity = 0.15f;
    public float topClamp = 70f;
    public float bottomClamp = -30f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private float _targetYaw;
    private float _targetPitch;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (cinemachineCameraTarget != null)
        {
            _targetYaw = cinemachineCameraTarget.eulerAngles.y;
            _targetPitch = cinemachineCameraTarget.localEulerAngles.x;
            _targetPitch = NormalizeAngle(_targetPitch);
        }
    }

    void FixedUpdate()
    {
        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = (camRight * moveInput.x + camForward * moveInput.y);

        Vector3 targetVel = move.normalized * moveSpeed;
        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(targetVel.x, vel.y, targetVel.z);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
        }
    }

    void LateUpdate()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        if (cinemachineCameraTarget == null) return;
        //Debug.Log("Camera Rotation: ");

        if (lookInput.sqrMagnitude > 0.0001f)
        {
            _targetYaw += lookInput.x * lookSensitivity;
            _targetPitch -= lookInput.y * lookSensitivity;

            _targetPitch = Mathf.Clamp(_targetPitch, bottomClamp, topClamp);
        }

        cinemachineCameraTarget.rotation = Quaternion.Euler(_targetPitch, _targetYaw, 0f);
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}