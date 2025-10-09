using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundColor : MonoBehaviour
{
    public Camera cam;

    void Start()
    {
        cam.clearFlags = CameraClearFlags.SolidColor;
    }
    void Update()
    {
        if (Input.GetKey("f"))
        {
            cam.backgroundColor = new Color(0f, 1f, 0f, 1f);
            Debug.Log("Background");
        }
        Debug.Log("" + cam.backgroundColor);
    }
}
