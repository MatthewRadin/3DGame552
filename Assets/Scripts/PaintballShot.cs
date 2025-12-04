using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;

public class PaintballShot : MonoBehaviour
{
    [SerializeField] private GameObject paintball;
    [SerializeField] private Transform camera;
    [SerializeField] private Transform gunLocation;
    [SerializeField] private LayerMask allButPlayerMask;

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

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        RaycastHit hit;

        Vector3 shootDirection;

        if (Physics.Raycast(ray, out hit, 1000f, allButPlayerMask))
        {
            shootDirection = (hit.point - gunLocation.position).normalized;
        }
        else
        {
            shootDirection = camera.transform.forward;
        }

        rb.linearVelocity = shootDirection * shootForce;

        Destroy(ball, 3f);

        GetComponent<PlayerMovement>().Recoil();
    }
}
