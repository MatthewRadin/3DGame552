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
        if (Mouse.current.leftButton.wasPressedThisFrame)
            ShootBall();
    }

    private void ShootBall()
    {
        GameObject ball = Instantiate(
            paintball,
            gunLocation.position,
            Quaternion.identity
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity = camera.forward * shootForce;

        Destroy(ball, 3f);

        GetComponent<PlayerMovement>().Recoil();
    }
}
