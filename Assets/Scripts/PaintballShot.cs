using UnityEngine;
using UnityEngine.InputSystem;

public class PaintballShot : MonoBehaviour
{
    [SerializeField] private GameObject paintball;
    [SerializeField] private Transform camera;
    [SerializeField] private Transform gunLocation;

    [SerializeField] private float shootForce = 25f;

    void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
            ShootBall();
    }

    private void ShootBall()
    {
        // Instantiate at the gun location, NOT as a child
        GameObject ball = Instantiate(
            paintball,
            gunLocation.position,
            Quaternion.identity
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        // Fire toward where the camera is pointing
        rb.linearVelocity = camera.forward * shootForce;

        Destroy(ball, 3f);
    }
}
