using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Manager;

namespace COM.Cam
{
    public class FreeCamera : MonoBehaviour
    {
        public static PlacementCamera instance { get; private set; }
        public static bool DisableRotation = false;

        /*Settings*/
        [Header("Settings")]
        public GameObject Center;
        public float CameraRotationSpeed = 100;
        public float CameraMoveSpeed = 5;

        void LateUpdate()
        {
            UpdateMovement();
            UpdateRotation();
        }

        void UpdateMovement()
        {
            Vector3 Movement = Vector3.zero;
            //Input
            if (Input.GetKey(OptionSettings.CameraForward)) Movement += new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
            if (Input.GetKey(OptionSettings.CameraBackward)) Movement -= new Vector3(transform.forward.x, transform.forward.y, transform.forward.z);
            if (Input.GetKey(OptionSettings.CameraLeft)) Movement -= new Vector3(transform.right.x, transform.right.y, transform.right.z);
            if (Input.GetKey(OptionSettings.CameraRight)) Movement += new Vector3(transform.right.x, transform.right.y, transform.right.z);

            //Move
            Center.transform.position += Movement.normalized * CameraMoveSpeed * Time.deltaTime;
        }

        void UpdateRotation()
        {
            //If right clicked
            if (Input.GetMouseButton(1) && !DisableRotation)
            {
                //Hides and lock mouse
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                //Input
                float MouseX = -Input.GetAxis("Mouse X") * CameraRotationSpeed * Time.deltaTime;
                float MouseY = -Input.GetAxis("Mouse Y") * CameraRotationSpeed * Time.deltaTime;

                //Rotation
                transform.RotateAround(Center.transform.position, transform.right, MouseY);
                transform.RotateAround(Center.transform.position, Center.transform.up, MouseX);
            }
            else
            {
                //Shows mouse
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            transform.LookAt(Center.transform.position);
        }
    }
}
