using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask playerLayer;

    [Header("Shooting")]
    [SerializeField] private GameObject paintball;
    [SerializeField] private float shootForce = 25f;

    private GameObject mainPlayer;
    private Vector3 mainPlayerPos, enemyPos, moveDir;
    private float shotTimer;
    private void Start()
    {
        mainPlayer = GameObject.FindGameObjectWithTag("Player");
    }
    private void Update()
    {
        // Find direction to main character
        mainPlayerPos = mainPlayer.transform.position;
        enemyPos = transform.position;
        moveDir = (new Vector3(mainPlayerPos.x - enemyPos.x, 0, mainPlayerPos.z - enemyPos.z)).normalized;

        // check if the spot towards them is in the air
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // if in the air, move enemy back
        if (!Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 2f, whatIsGround))
            transform.position = enemyPos;

        // If their is a way to see the player, shoot a shot in their direction, Update a shot counter so that they can't shoot too often
        if (shotTimer <= 0 && Physics.Raycast(transform.position, mainPlayerPos - enemyPos, 25f, playerLayer))
        {
            ShootShot();
            shotTimer = 1f;
        }
        shotTimer -= Time.deltaTime;
    }
    private void ShootShot()
    {
        GameObject ball = Instantiate(
            paintball,
            transform.position,
            Quaternion.identity
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity = (mainPlayerPos - enemyPos).normalized * shootForce;

        Destroy(ball, 3f);
    }
}
