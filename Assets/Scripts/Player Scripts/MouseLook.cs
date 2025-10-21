using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 1000f;
    public Transform playerBody; 
    private float xRotation = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //Locking the cursor to the center of the screen
    }

    // Update is called once per frame
    void Update()
    {
        //Getting input from mouse for X and Y axis
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //Clamping the rotation so we don't flip over
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        //Vecotr3.up is the Y axis that we rotate upon
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
