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
    [SerializeField] private float viewDistance = 25f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private GameObject mainPlayer;
    private Rigidbody rb;
    private Vector3 mainPlayerPos, enemyPos, moveDirection;
    private float shotTimer;
    private bool isAlive = true;
    private void Awake()
    {
        GameObject.FindGameObjectWithTag("LevelManager").gameObject.GetComponent<LevelManager>().IncrementEnemy();
    }
    private void Start()
    {
        mainPlayer = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (!isAlive) return;
        mainPlayerPos = mainPlayer.transform.position;
        enemyPos = transform.position;
        moveDirection = (new Vector3(mainPlayerPos.x - enemyPos.x, 0, mainPlayerPos.z - enemyPos.z)).normalized;

        Vector3 lookDir = mainPlayerPos - enemyPos;
        lookDir = -lookDir;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
        if (OnSlope())
            transform.position += GetSlopeMoveDirection(moveDirection) * moveSpeed * Time.deltaTime;
        else
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (!Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 2f, whatIsGround))
            transform.position = enemyPos;

        if (shotTimer <= 0)
        {
            RaycastHit hit;
            Vector3 direction = mainPlayerPos - enemyPos;

            if (Physics.Raycast(transform.position, direction.normalized, out hit, viewDistance, ~0))
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
        animator.SetTrigger("Shoot");
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
            GameObject.FindGameObjectWithTag("LevelManager").gameObject.GetComponent<LevelManager>().EnemyKilled();
            animator.SetBool("isDead", true);
            GetComponent<CapsuleCollider>().enabled = false;
            Destroy(collision.gameObject);
            Destroy(this.gameObject, 1f);
        }
    }
}
