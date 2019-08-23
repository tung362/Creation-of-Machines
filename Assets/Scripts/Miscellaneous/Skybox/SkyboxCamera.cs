using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxCamera : MonoBehaviour
{
    Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
    }
    void LateUpdate()
    {
        camera.fieldOfView = Camera.main.fieldOfView;
        transform.rotation = Camera.main.transform.rotation;
    }
}
