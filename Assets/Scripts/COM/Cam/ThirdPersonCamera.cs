using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.Cam
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public GameObject aimTarget;//Replace this later with a vector on a mob script that the script pulls from
        public Camera PlayerCamera;
        public float CameraRotationSpeed = 100;
        public float CameraMaxYRotation = 85;
        public float CameraMinYRotation = -39;

        public float DesiredCameraDistance = 3.319367f;
        public float CameraZoomSmoothness = 0.2f;
        private float ZoomVelocity = 0;
        private float MouseWheelValue = 0;

        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void LateUpdate()
        {
            UpdateRotation();
            CheckSolidCollision();
        }

        void CheckSolidCollision()
        {
            //Raycast from aim target to camera
            Vector3 cameraDirection = PlayerCamera.transform.position - aimTarget.transform.position;
            Ray collisionCheckRay = new Ray(aimTarget.transform.position, cameraDirection);
            float collisionDistance = CheckCollision(collisionCheckRay, 0.25f); //Cushion
            float currentCameraDistance = Vector3.Distance(aimTarget.transform.position, PlayerCamera.transform.position);

            //Collision adjustment
            if (currentCameraDistance - collisionDistance > 0) PlayerCamera.transform.position = collisionCheckRay.GetPoint(collisionDistance);
            else
            {
                float smooth = Mathf.SmoothDamp(0, currentCameraDistance - collisionDistance, ref ZoomVelocity, 0.2f);
                PlayerCamera.transform.position += PlayerCamera.transform.forward * smooth;
            }
        }

        void UpdateRotation()
        {
            //Input
            float MouseX = Input.GetAxis("Mouse X") * CameraRotationSpeed * Time.deltaTime;
            float MouseY = -Input.GetAxis("Mouse Y") * CameraRotationSpeed * Time.deltaTime;

            //Y angle calculation (90 to -90)
            float angleY = Vector3.Angle(new Vector3(PlayerCamera.transform.forward.x, 0, PlayerCamera.transform.forward.z), PlayerCamera.transform.forward);
            float dotY = Vector3.Dot(Vector3.up, PlayerCamera.transform.forward);
            if (dotY > 0) angleY = -angleY;

            //Clamp Y rotation
            float nextAngleY = angleY + MouseY;
            if (nextAngleY < CameraMinYRotation) MouseY = CameraMinYRotation - angleY;
            else if (nextAngleY > CameraMaxYRotation) MouseY = CameraMaxYRotation - angleY;

            //Rotation
            PlayerCamera.transform.RotateAround(aimTarget.transform.position, PlayerCamera.transform.right, MouseY);
            PlayerCamera.transform.RotateAround(aimTarget.transform.position, Vector3.up, MouseX);
            PlayerCamera.transform.LookAt(aimTarget.transform, Vector3.up);
        }

        private float CheckCollision(Ray ray, float wallCushion)
        {
            RaycastHit[] hits = Physics.SphereCastAll(ray, wallCushion, DesiredCameraDistance);
            float closestDistance = DesiredCameraDistance;
            for (int i = 0; i < hits.Length; i++)
            {
                float distance = hits[i].distance;
                if (distance < closestDistance) closestDistance = distance;
            }
            return closestDistance;
        }
    }
}
