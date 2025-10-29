using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CharacterController : MonoBehaviour
{
	public float moveAcceleration = 20f, maxSpeed = 8f;
	public bool snappy = true;
	public float decelerationTime = 0f;
	public float coyoteTime = 0.12f, jumpBufferTime = 0.12f;
	float coyoteTimer = 0f, jumpBufferTimer = 0f;
	[Range(0f, 1f)] public float movementBlend = 1f;
	float externalLock = 0f, externalLockDur = 0f;
	public bool allowJump = true; public float jumpForce = 6f;
	public LayerMask groundMask = ~0; public float groundCheckDistance = 0.51f;

	Rigidbody rb; Camera cam; Vector2 moveInput; bool jumpRequested;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		cam = Camera.main;
	}

	void Update()
	{
		Vector2 kb = Vector2.zero;
		if (Keyboard.current != null)
		{
			float h = 0f, v = 0f;
			if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
			if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
			if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;
			if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
			kb = new Vector2(h, v);
			if (allowJump && Keyboard.current.spaceKey != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.spaceKey.isPressed))
			{
				jumpRequested = true; jumpBufferTimer = jumpBufferTime;
			}
		}

		Vector2 gp = Vector2.zero;
		if (Gamepad.current != null)
		{
			gp = Gamepad.current.leftStick.ReadValue();
			if (allowJump && Gamepad.current.buttonSouth.wasPressedThisFrame)
			{
				jumpRequested = true; jumpBufferTimer = jumpBufferTime;
			}
		}

		moveInput = (Gamepad.current != null && gp.sqrMagnitude > 0.001f) ? gp : kb;
	}

	void FixedUpdate()
	{
		if (IsGrounded()) coyoteTimer = coyoteTime; else coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);
		jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.fixedDeltaTime);
		externalLock = Mathf.Max(0f, externalLock - Time.fixedDeltaTime);

		Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);

		if (snappy)
		{
			Vector3 cur = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
			float blend = movementBlend;
			if (externalLock > 0f && externalLockDur > 0f) blend *= 1f - (externalLock / externalLockDur);
			if (input.sqrMagnitude > 0.0001f)
			{
				Vector3 dir = GetCameraRelative(input.normalized);
				Vector3 targ = dir * maxSpeed;
				Vector3 next = Vector3.Lerp(cur, targ, blend);
				rb.linearVelocity = new Vector3(next.x, rb.linearVelocity.y, next.z);
			}
			else
			{
				if (decelerationTime <= 0f) rb.linearVelocity = new Vector3(Mathf.Lerp(cur.x, 0f, blend), rb.linearVelocity.y, Mathf.Lerp(cur.z, 0f, blend));
				else
				{
					Vector3 decel = Vector3.MoveTowards(cur, Vector3.zero, (cur.magnitude / Mathf.Max(0.0001f, decelerationTime)) * Time.fixedDeltaTime);
					Vector3 next = Vector3.Lerp(cur, decel, blend);
					rb.linearVelocity = new Vector3(next.x, rb.linearVelocity.y, next.z);
				}
			}
		}
		else
		{
			if (input.sqrMagnitude > 0.0001f) rb.AddForce(GetCameraRelative(input.normalized) * moveAcceleration, ForceMode.Acceleration);
			Vector3 vel = rb.linearVelocity; Vector3 horiz = new Vector3(vel.x, 0f, vel.z);
			if (horiz.magnitude > maxSpeed) { Vector3 c = horiz.normalized * maxSpeed; rb.linearVelocity = new Vector3(c.x, vel.y, c.z); }
		}

		bool canJump = coyoteTimer > 0f; bool buffered = jumpRequested || jumpBufferTimer > 0f;
		if (allowJump && buffered && canJump)
		{
			Vector3 v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v; rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
			jumpRequested = false; jumpBufferTimer = 0f; coyoteTimer = 0f;
		}
	}

	Vector3 GetCameraRelative(Vector3 inDir)
	{
		if (cam == null) return transform.TransformDirection(inDir);
		Vector3 f = cam.transform.forward; f.y = 0f; f.Normalize(); Vector3 r = cam.transform.right; r.y = 0f; r.Normalize();
		Vector3 m = r * inDir.x + f * inDir.z; if (m.sqrMagnitude > 1f) m.Normalize(); return m;
	}

	bool IsGrounded()
	{
		// Capsule: compute bottom point in world space then raycast down a short distance
		if (TryGetComponent<CapsuleCollider>(out var cap))
		{
			Vector3 centerWS = cap.transform.TransformPoint(cap.center);
			float scaleY = cap.transform.lossyScale.y;
			float scaleX = Mathf.Max(cap.transform.lossyScale.x, cap.transform.lossyScale.z);
			float radiusWS = cap.radius * scaleX;
			float halfHeightWS = (cap.height * 0.5f) * scaleY;
			float distToBottom = Mathf.Max(0f, halfHeightWS - radiusWS);
			Vector3 bottom = centerWS + Vector3.down * distToBottom;
			return Physics.Raycast(bottom + Vector3.up * 0.01f, Vector3.down, groundCheckDistance + 0.01f, groundMask, QueryTriggerInteraction.Ignore);
		}

		// Sphere: cast a short ray from the sphere bottom
		if (TryGetComponent<SphereCollider>(out var sph))
		{
			Vector3 centerWS = sph.transform.TransformPoint(sph.center);
			float radiusWS = sph.radius * sph.transform.lossyScale.y;
			Vector3 bottom = centerWS + Vector3.down * radiusWS;
			return Physics.Raycast(bottom + Vector3.up * 0.01f, Vector3.down, groundCheckDistance + 0.01f, groundMask, QueryTriggerInteraction.Ignore);
		}

		// Fallback: raycast from transform position
		return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
	}

	public void ApplyKnockback(Vector3 impulse, float lockDur = 0.5f) { if (rb == null) rb = GetComponent<Rigidbody>(); rb.AddForce(impulse, ForceMode.VelocityChange); externalLock = lockDur; externalLockDur = lockDur; }

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		if (TryGetComponent<CapsuleCollider>(out var cap)) { var b = cap.bounds; Vector3 bottom = new Vector3(b.center.x, b.min.y + 0.01f, b.center.z); float r = Mathf.Max(0.05f, Mathf.Min(b.extents.x, b.extents.z) * 0.9f); Gizmos.DrawWireSphere(bottom, r); }
		else if (TryGetComponent<SphereCollider>(out var sph)) { var b = sph.bounds; Vector3 bottom = new Vector3(b.center.x, b.min.y + 0.01f, b.center.z); float r = Mathf.Max(0.05f, b.extents.x * 0.9f); Gizmos.DrawWireSphere(bottom, r); }
		else Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
	}
}
