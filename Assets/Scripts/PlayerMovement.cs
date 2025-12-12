using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Recoil Handling")]
    [SerializeField] private float recoilForce = 10f;
    [SerializeField] private bool resetYVelocity = true;

    [Header("Velocity Smoothing")]
    [Tooltip("How fast horizontal velocity is lerped toward the allowed speed. Higher = quicker.")]
    [SerializeField] private float velocityLerpSpeed = 6f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    public MovementState state;
    public enum MovementState
    {
        walking,
        air,
        sliding
    }

    public bool sliding;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        moveSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = 0;
        verticalInput = 0;

        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;
        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;

        if (Keyboard.current.spaceKey.isPressed && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StateHandler()
    {
        // SLIDING
        if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = walkSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;

            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;

            desiredMoveSpeed = walkSpeed;
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (!grounded && sliding) rb.linearDamping = groundDrag;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            Vector3 current = rb.linearVelocity;
            float currentMag = current.magnitude;
            if (currentMag > 0.0001f)
            {
                Vector3 target = current.normalized * Mathf.Min(currentMag, moveSpeed);
                Vector3 newVel = Vector3.Lerp(current, target, Time.deltaTime * velocityLerpSpeed);
                rb.linearVelocity = newVel;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.sqrMagnitude > 0.00001f)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;

                if (limitedVel.magnitude < flatVel.magnitude)
                {
                    Vector3 newFlat = Vector3.Lerp(flatVel, limitedVel, Time.deltaTime * velocityLerpSpeed);
                    rb.linearVelocity = new Vector3(newFlat.x, rb.linearVelocity.y, newFlat.z);
                }
                else if (flatVel.magnitude < limitedVel.magnitude)
                {
                    Vector3 newFlat = Vector3.Lerp(flatVel, limitedVel, Time.deltaTime * (velocityLerpSpeed * 0.5f));
                    rb.linearVelocity = new Vector3(newFlat.x, rb.linearVelocity.y, newFlat.z);
                }
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        SfxController.PlayPlayerJump();

        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    public void Recoil()
    {
        Vector3 flatBefore = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (resetYVelocity)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 backward = -orientation.forward;
        backward.Normalize();
        backward.y = backward.y / 1.5f;

        Vector3 impulse = backward * recoilForce;
        rb.AddForce(impulse, ForceMode.Impulse);

        rb.linearVelocity = new Vector3(flatBefore.x, rb.linearVelocity.y, flatBefore.z);
    }
}
