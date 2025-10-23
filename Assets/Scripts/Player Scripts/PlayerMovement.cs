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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        if(!isGrounded && transform.position.y < respawnHeight)
        {
            controller.enabled = false;

            transform.position = spawnPoint.position;

            controller.enabled = true;
        }
;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * x + transform.forward * z;

        controller.Move(movement * speed * Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jump * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    
    }
}
