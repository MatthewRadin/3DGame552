using UnityEngine;
using UnityEngine.InputSystem;

public class MouseRotationController : MonoBehaviour
{
    [SerializeField] private Transform virtualCamera;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float stickSensitivity = 150f;
    [SerializeField] private bool useDamping = true;
    [SerializeField] private float dampingTime = 0.08f;

    private float _yaw;
    private float _pitch;
    private float targetYaw;
    private float targetPitch;
    private float yawVelocity;
    private float pitchVelocity;

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera not assigned to MouseRotationController!");
            enabled = false;
            return;
        }
        Vector3 currentEuler = virtualCamera.transform.rotation.eulerAngles;
        _yaw = currentEuler.y;
        _pitch = currentEuler.x;
    }

    private void Update()
    {
    Vector2 mouseDelta = Vector2.zero;
        if (Mouse.current != null)
            mouseDelta = Mouse.current.delta.ReadValue();

        Vector2 stick = Vector2.zero;
        if (Gamepad.current != null)
            stick = Gamepad.current.rightStick.ReadValue();

        if (stick.sqrMagnitude > 0.001f)
        {
            targetYaw += stick.x * stickSensitivity * Time.deltaTime;
            targetPitch -= stick.y * stickSensitivity * Time.deltaTime;
        }
        else
        {
            targetYaw += mouseDelta.x * mouseSensitivity * rotationSpeed;
            targetPitch -= mouseDelta.y * mouseSensitivity * rotationSpeed;
        }

        targetPitch = Mathf.Clamp(targetPitch, -90f, 90f);

        if (useDamping)
        {
            _yaw = Mathf.SmoothDampAngle(_yaw, targetYaw, ref yawVelocity, dampingTime);
            _pitch = Mathf.SmoothDampAngle(_pitch, targetPitch, ref pitchVelocity, dampingTime);
        }
        else
        {
            _yaw = targetYaw;
            _pitch = targetPitch;
        }

        virtualCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
