using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Controller Setup")]
    public CharacterController controller;
    public Transform spawnPoint;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Movement Feel")]
    [Tooltip("How fast you reach top speed. High = Snappy start.")]
    [SerializeField] private float groundAcceleration = 60f;

    [Tooltip("How fast you stop. Lower = More Slide.")]
    [SerializeField] private float groundDeceleration = 25f;

    [Tooltip("How fast you can change direction in the air.")]
    [SerializeField] private float airAcceleration = 20f;

    // State
    public bool isGrounded;
    private bool inGeyser = false;
    private float geyserLiftForce = 0f;

    // Velocity Tracking
    private Vector3 verticalVelocity;
    private Vector3 horizontalVelocity;

    private RuntimePlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<RuntimePlayerStats>();
        if (playerStats == null) Debug.LogError("RuntimePlayerStats component not found!");
    }

    public void SetGeyserState(bool active, float liftForce)
    {
        inGeyser = active;
        geyserLiftForce = liftForce;

        if (active)
        {
            if (verticalVelocity.y < 0) verticalVelocity.y = 0f;
        }
    }

    public void LaunchPlayer(float force)
    {
        verticalVelocity.y = force;
        isGrounded = false;
    }

    void Update()
    {
        if (playerStats == null) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 1. Vertical Logic
        HandleVerticalMovement();

        // 2. Horizontal Logic
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = (transform.right * x + transform.forward * z).normalized;
        Vector3 targetVelocity = inputDir * playerStats.speed;

        if (isGrounded)
        {
            // Determine if we are accelerating (inputting) or stopping (no input)
            // If inputting, use high acceleration (snappy). If stopping, use deceleration (slide).
            float speedChangeRate = (inputDir.magnitude > 0.1f) ? groundAcceleration : groundDeceleration;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                speedChangeRate * Time.deltaTime
            );
        }
        else
        {
            // Air / Geyser Control
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                targetVelocity,
                airAcceleration * Time.deltaTime
            );
        }

        // 3. Respawn Check
        if (transform.position.y < playerStats.respawnHeight)
        {
            Respawn();
        }

        // 4. Apply Final Move
        Vector3 finalMovement = horizontalVelocity + verticalVelocity;
        controller.Move(finalMovement * Time.deltaTime);
    }

    private void HandleVerticalMovement()
    {
        if (inGeyser)
        {
            verticalVelocity.y += geyserLiftForce * Time.deltaTime;

            float maxLiftSpeed = 25f;
            if (verticalVelocity.y > maxLiftSpeed) verticalVelocity.y = maxLiftSpeed;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity.y += Mathf.Sqrt(playerStats.jump * 3f * -playerStats.gravity);
            }
        }
        else
        {
            if (isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f;
            }

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                verticalVelocity.y = Mathf.Sqrt(playerStats.jump * -2f * playerStats.gravity);
            }

            verticalVelocity.y += playerStats.gravity * Time.deltaTime;
        }
    }

    private void Respawn()
    {
        controller.enabled = false;
        transform.position = spawnPoint.position;
        verticalVelocity = Vector3.zero;
        horizontalVelocity = Vector3.zero;
        controller.enabled = true;
    }
}