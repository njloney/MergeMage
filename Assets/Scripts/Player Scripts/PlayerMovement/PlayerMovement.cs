using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public CharacterController controller;

    private RuntimePlayerStats playerStats;

    public Transform spawnPoint;

    public Transform groundCheck;

    public float groundDistance = 0.4f;

    public LayerMask groundMask;

    public bool isGrounded;

    Vector3 velocity;
    Vector3 horizontalMovement;

    void Start()
    {
        playerStats = GetComponent<RuntimePlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("RuntimePlayerStats component not found on Player!");
        }
    }

    void Update()
    {
        if (playerStats == null)
        {
            Debug.LogError("Player Stats not assigned!");
            return;
        }

        // Check for ground status
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Always check for input, whether on ground or in air
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calculate horizontal movement based on input (now uses runtime stats)
        horizontalMovement = (transform.right * x + transform.forward * z) * playerStats.speed;

        if (isGrounded)
        {
            // Reset vertical velocity if we just landed
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Only allow jumping if we are on the ground
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(playerStats.jump * -2f * playerStats.gravity);
            }
        }

        // Respawn if player went off the map
        if (transform.position.y < playerStats.respawnHeight)
        {
            controller.enabled = false;
            transform.position = spawnPoint.position;
            controller.enabled = true;
            velocity.y = 0f;
        }

        // Applying gravity 
        velocity.y += playerStats.gravity * Time.deltaTime;

        // Move the controller
        controller.Move((horizontalMovement + velocity) * Time.deltaTime);
    }
}