using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public CharacterController controller;

    private Stats playerStats;

    public Transform spawnPoint;

    public float respawnHeight = -10f;

    public Transform groundCheck;

    public float groundDistance = 0.4f;

    public LayerMask groundMask;

    public bool isGrounded;

    Vector3 velocity;
    Vector3 horizontalMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         playerStats = Resources.Load<Stats>("PlayerResources/PlayerStats");
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStats == null)
        {
            Debug.LogError("Player Stats asset is not assigned!");
            return; // Stop running if stats are missing
        }

        // Check for ground status
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Always check for input, whether on ground or in air
        float x = Input.GetAxis("Horizontal"); //Getting horizontal input
        float z = Input.GetAxis("Vertical"); //Getting vertical input

        // Calculate horizontal movement based on input
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
        
        else
        {
           
        }

        // Respawn if player went off the map
        if(transform.position.y < respawnHeight)
        {
            controller.enabled = false;
            transform.position = spawnPoint.position;
            controller.enabled = true;
            velocity.y = 0f; // Reset velocity on respawn
        }

        // Applying gravity 
        velocity.y += playerStats.gravity * Time.deltaTime;

        // Move the controller
        // Apply horizontal movement and vertical (gravity/jump) velocity together
        controller.Move((horizontalMovement + velocity) * Time.deltaTime);       
    }
}
