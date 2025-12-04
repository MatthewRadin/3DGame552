using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask playerLayer;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Shooting")]
    [SerializeField] private GameObject paintball;
    [SerializeField] private float shootForce = 25f;

    private GameObject mainPlayer;
    private Rigidbody rb;
    private Vector3 mainPlayerPos, enemyPos, moveDirection;
    private float shotTimer;
    private void Start()
    {
        mainPlayer = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        // Find direction to main character
        mainPlayerPos = mainPlayer.transform.position;
        enemyPos = transform.position;
        moveDirection = (new Vector3(mainPlayerPos.x - enemyPos.x, 0, mainPlayerPos.z - enemyPos.z)).normalized;

        if (OnSlope())
        {
            transform.position += GetSlopeMoveDirection(moveDirection) * moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        // if in the air, move enemy back
        if (!Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 2f, whatIsGround))
            transform.position = enemyPos;

        // If their is a way to see the player, shoot a shot in their direction, Update a shot counter so that they can't shoot too often
        if (shotTimer <= 0)
        {
            RaycastHit hit;
            Vector3 direction = mainPlayerPos - enemyPos;

            if (Physics.Raycast(transform.position, direction.normalized, out hit, 25f, ~0)) // all layers
            {
                if (hit.transform.CompareTag("Player"))
                {
                    ShootShot();
                    shotTimer = 1f;
                }
            }
        }

        shotTimer -= Time.deltaTime;
    }
    private void ShootShot()
    {
        GameObject ball = Instantiate(
            paintball,
            transform.position + ((mainPlayerPos - enemyPos).normalized * 2),
            Quaternion.identity
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.linearVelocity = (mainPlayerPos - enemyPos).normalized * shootForce;

        Destroy(ball, 3f);
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
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("PlayerBullet")) //Mods, kill this bean
        {
            Destroy(collision.gameObject);
            Destroy(this.gameObject);
        }
    }
}
