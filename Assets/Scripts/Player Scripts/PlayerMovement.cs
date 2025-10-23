using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public CharacterController controller;


    public Transform spawnPoint;

    public float respawnHeight = -10f;

    public float speed = 12f;

    public float gravity = -9.81f;

    public float jump = 3f;

    public Transform groundCheck;

    public float groundDistance = 0.4f;

    public LayerMask groundMask;
    public LayerMask cubeMask;

    public bool isGrounded;

    Vector3 velocity;
    Vector3 horizontalMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);


        if (isGrounded)
        {

            float x = Input.GetAxis("Horizontal"); //Getting horizontal input
            float z = Input.GetAxis("Vertical"); //Getting vertical input

            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }

            horizontalMovement = (transform.right * x + transform.forward * z) * speed;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jump * -2f * gravity);

            }

        }
        else
        {
            //We are not reading movement here

        }
        
        if(!isGrounded && transform.position.y < respawnHeight)
        {
            controller.enabled = false;

            transform.position = spawnPoint.position;

            controller.enabled = true;
        }

        //applying gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move((horizontalMovement + velocity) * Time.deltaTime);

        //respawn if player went off the map
        
;

        
       
    }
}
