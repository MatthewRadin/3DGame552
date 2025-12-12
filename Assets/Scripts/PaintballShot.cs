using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;
using UnityEngine.UI;

public class PaintballShot : MonoBehaviour
{
    [SerializeField] private GameObject paintball;
    [SerializeField] private Transform camera;
    [SerializeField] private Transform gunLocation;
    [SerializeField] private LayerMask allButPlayerMask;

    [SerializeField] private float shootForce = 25f;

    [SerializeField] private Slider timerSlider;

    private float shotTimer = 1f;

    void Update()
    {
        timerSlider.value = shotTimer;
        shotTimer += Time.deltaTime;
        if (!MenuManager.isGamePaused && shotTimer > 1f && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootBall();
            shotTimer = 0f;
        }
    }

    private void ShootBall()
    {
        SfxController.PlayPlayerShoot();
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
