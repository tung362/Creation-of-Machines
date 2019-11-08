using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM
{
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
}
