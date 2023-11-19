using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera mainCamera;

    void Start()
    {
        // Find and store a reference to the main camera
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Make sure the object faces the camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
