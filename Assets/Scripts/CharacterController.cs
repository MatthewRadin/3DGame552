using UnityEngine;
// Use the new Input System
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CharacterController : MonoBehaviour
{
	[Header("Movement")]
	[Tooltip("Acceleration force applied for movement")]
	public float moveAcceleration = 20f;
	[Tooltip("Maximum horizontal speed (m/s)")]
	public float maxSpeed = 8f;

	[Header("Jump")]
	public bool allowJump = true;
	public float jumpForce = 6f;

	[Header("Ground Check")]
	public LayerMask groundMask = ~0; // default to everything
	[Tooltip("Distance from sphere center to check for ground contact")]
	public float groundCheckDistance = 0.51f;

	Rigidbody rb;
	Camera mainCamera;

	// Input System state
	Vector2 moveInput = Vector2.zero; // x = horizontal (A/D), y = vertical (W/S)
	bool jumpRequested = false; // set in Update when jump pressed, consumed in FixedUpdate

	bool isGrounded => Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		// We expect a sphere: freeze rotation only if you want no tumbling. For a rolling sphere, do not freeze.
		// rb.freezeRotation = true; // leave commented to allow natural rolling

		mainCamera = Camera.main;
	}

	void Update()
	{
		// Read inputs using the new Input System. We sample both keyboard and gamepad so either works.
		Vector2 kb = Vector2.zero;

		if (Keyboard.current != null)
		{
			float h = 0f;
			float v = 0f;
			if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
			if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
			if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
			if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
			kb = new Vector2(h, v);

			if (allowJump && Keyboard.current.spaceKey != null && Keyboard.current.spaceKey.wasPressedThisFrame)
				jumpRequested = true;
		}

		Vector2 gp = Vector2.zero;
		if (Gamepad.current != null)
		{
			gp = Gamepad.current.leftStick.ReadValue();
			// gamepad south button as jump (A on Xbox)
			if (allowJump && Gamepad.current.buttonSouth.wasPressedThisFrame)
				jumpRequested = true;
		}

		// Prefer gamepad if active, otherwise keyboard
		if (Gamepad.current != null && gp.sqrMagnitude > 0.001f)
			moveInput = gp;
		else
			moveInput = kb;
	}

	void FixedUpdate()
	{
		// Use moveInput populated in Update (supports new Input System)
		Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

		if (input.sqrMagnitude > 0.0001f)
		{
			Vector3 moveDir = GetCameraRelativeDirection(input.normalized);

			// Project current velocity onto horizontal plane
			Vector3 horizontalVel = rb.linearVelocity;
			horizontalVel.y = 0f;

			// If under max speed, apply acceleration
			if (horizontalVel.magnitude < maxSpeed || Vector3.Dot(horizontalVel.normalized, moveDir) < 0f)
			{
				rb.AddForce(moveDir * moveAcceleration, ForceMode.Acceleration);
			}
		}

		// Limit horizontal speed
		Vector3 vel = rb.linearVelocity;
		Vector3 horiz = new Vector3(vel.x, 0f, vel.z);
		if (horiz.magnitude > maxSpeed)
		{
			Vector3 capped = horiz.normalized * maxSpeed;
			rb.linearVelocity = new Vector3(capped.x, vel.y, capped.z);
		}

		// Jump handling: consume jumpRequested set in Update
		if (allowJump && jumpRequested && isGrounded)
		{
			// zero vertical velocity first for consistent jumps
			Vector3 vNow = rb.linearVelocity;
			vNow.y = 0f;
			rb.linearVelocity = vNow;
			rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
			jumpRequested = false;
		}

		// If jump was requested but we weren't grounded yet, keep the request for a short buffer until next FixedUpdate
	}

	Vector3 GetCameraRelativeDirection(Vector3 inputDir)
	{
		if (mainCamera == null)
		{
			// no camera found: use world-relative
			return transform.TransformDirection(inputDir);
		}

		// Camera forward on XZ plane
		Vector3 camForward = mainCamera.transform.forward;
		camForward.y = 0f;
		camForward.Normalize();

		Vector3 camRight = mainCamera.transform.right;
		camRight.y = 0f;
		camRight.Normalize();

		Vector3 move = camRight * inputDir.x + camForward * inputDir.z;
		if (move.sqrMagnitude > 1f) move.Normalize();
		return move;
	}

	void OnDrawGizmosSelected()
	{
		// draw ground check ray
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
	}
}
