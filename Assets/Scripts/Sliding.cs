using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    // INPUT
    private float horizontalInput;
    private float verticalInput;

    private bool sliding;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }
    private void Update()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;
        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;

        if (Keyboard.current.commaKey.wasPressedThisFrame)
            StartSlide();

        if (Keyboard.current.commaKey.wasReleasedThisFrame)
            StopSlide();
    }
    private void FixedUpdate()
    {
        if (sliding)
            SlidingMovement();
    }
    private void StartSlide()
    {
        sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }
    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        inputDirection.y = 0f;

        rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0)
            StopSlide();
    }
    private void StopSlide()
    {
        sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
