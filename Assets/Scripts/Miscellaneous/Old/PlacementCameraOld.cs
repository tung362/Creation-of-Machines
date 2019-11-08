using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM
{
    public class PlacementCameraOld : MonoBehaviour
    {
        /*Settings*/
        [Header("Settings")]
        public GameObject Center;
        public float CameraRotationSpeed = 100;
        public float CameraMoveSpeed = 5;
        public float CameraMaxYRotation = 85;
        public float CameraMinYRotation = -39;
        public float CameraMaxZoom = 2;
        public float CameraMinZoom = 6;
        public float CameraZoomSpeed = 200;
        public float CameraZoomSmoothness = 0.2f;
        public float CameraElevationHeightCheck = 10.0f;
        public float CameraElevationSmoothness = 0.3f;
        public float CameraFogSmoothness = 0.3f;
        public bool Reflection = true;
        public bool DepthOfField = true;
        public bool Fog = true;

        [Header("Reflection Probe")]
        public ReflectionProbe Probe;

        [Header("Depth of Field")]
        public GameObject FocusObject;
        //Get values from post processing controller depth of field
        public float MinFocusDistance = 1.77f;
        public float MaxFocusDistance = 3.03f;

        [Header("Fog")]
        //Get values from post processing controller depth of field
        public float MinDensity = 1.77f;
        public float MaxDensity = 3.03f;

        //[Header("Borders")]
        //public float BorderDistanceFadeThreshold = 4;
        //public float BorderMaxFadeThreshold = 2;
        ////0 = Front, 1 = Back, 2 = Right, 3 = Left
        //public GameObject[] Borders;

        /*Data*/
        //Zoom
        private float MouseWheelValue = 0;
        private float ZoomVelocity = 0;
        //Elevation
        private Vector3 ElevationVelocity = Vector3.zero;
        //Fog
        private float FogVelocity = 0;
        //Border
        private float BorderMaxAlpha = 0.31f;
        private Color BorderMaxEmission = Color.black;
        private Material[] BorderMaterials;

        //void Awake()
        //{
        //    //Fades all borders and get alpha value
        //    if (Borders.Length != 0)
        //    {
        //        BorderMaterials = new Material[Borders.Length];
        //        BorderMaxAlpha = Borders[0].GetComponent<Renderer>().material.color.a;
        //        BorderMaxEmission = Borders[0].GetComponent<Renderer>().material.GetColor("_EmissionColor");
        //        for (int i = 0; i < Borders.Length; i++)
        //        {
        //            BorderMaterials[i] = Borders[i].GetComponent<Renderer>().material;
        //            BorderMaterials[i].color = new Color(BorderMaterials[i].color.r, BorderMaterials[i].color.g, BorderMaterials[i].color.b, 0);
        //        }
        //    }
        //}

        void LateUpdate()
        {
            UpdateMovement();
            UpdateZoom();
            UpdateRotation();
            UpdateProbe();
            //UpdateDepthOfField();
            //UpdateBorders();
        }

        void UpdateMovement()
        {
            Vector3 Movement = Vector3.zero;
            //Input
            if (Input.GetKey(OptionSettings.CameraForward)) Movement += new Vector3(transform.forward.x, 0, transform.forward.z);
            if (Input.GetKey(OptionSettings.CameraBackward)) Movement -= new Vector3(transform.forward.x, 0, transform.forward.z);
            if (Input.GetKey(OptionSettings.CameraLeft)) Movement -= new Vector3(transform.right.x, 0, transform.right.z);
            if (Input.GetKey(OptionSettings.CameraRight)) Movement += new Vector3(transform.right.x, 0, transform.right.z);

            //Limits movement
            if (Center.transform.position.z > GameSettings.MapDimension.y && Movement.z > 0 || Center.transform.position.z < -GameSettings.MapDimension.y && Movement.z < 0) Movement.z = 0;
            if (Center.transform.position.x > GameSettings.MapDimension.x && Movement.x > 0 || Center.transform.position.x < -GameSettings.MapDimension.x && Movement.x < 0) Movement.x = 0;

            //Move
            Center.transform.position += Movement.normalized * CameraMoveSpeed * Time.deltaTime;

            //Elevation
            RaycastHit centerHit;
            RaycastHit cameraHit;
            float centerElevation = -int.MaxValue;
            float cameraElevation = -int.MaxValue;
            if (Physics.Raycast(new Vector3(Center.transform.position.x, CameraElevationHeightCheck, Center.transform.position.z), Vector3.down, out centerHit)) centerElevation = centerHit.point.y;
            if (Physics.Raycast(new Vector3(transform.position.x, CameraElevationHeightCheck, transform.position.z), Vector3.down, out cameraHit)) cameraElevation = cameraHit.point.y;

            if (centerElevation >= cameraElevation) Center.transform.position = Vector3.SmoothDamp(Center.transform.position, new Vector3(Center.transform.position.x, centerElevation, Center.transform.position.z), ref ElevationVelocity, CameraElevationSmoothness);
            else Center.transform.position = Vector3.SmoothDamp(Center.transform.position, new Vector3(Center.transform.position.x, cameraElevation, Center.transform.position.z), ref ElevationVelocity, CameraElevationSmoothness);
        }

        void UpdateZoom()
        {
            float distance = Vector3.Distance(Center.transform.position, transform.position + transform.forward * MouseWheelValue);
            //Input
            float MouseWheel = Input.GetAxis("Mouse ScrollWheel") * CameraZoomSpeed * Time.deltaTime;

            //Clamp zoom
            MouseWheel = Mathf.Clamp(MouseWheel, -1, 1);

            //Limit zoom
            if ((distance > CameraMaxZoom && distance < CameraMinZoom) ||
                (distance < CameraMaxZoom && MouseWheel < 0) ||
                (distance > CameraMinZoom && MouseWheel > 0)) MouseWheelValue += MouseWheel;

            //Smooth
            float smooth = Mathf.SmoothDamp(0, MouseWheelValue, ref ZoomVelocity, CameraZoomSmoothness);
            MouseWheelValue = MouseWheelValue - smooth;

            //Zoom
            transform.position += transform.forward * smooth;
        }

        void UpdateRotation()
        {
            //If right clicked
            if (Input.GetMouseButton(1))
            {
                //Hides and lock mouse
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                //Input
                float MouseX = -Input.GetAxis("Mouse X") * CameraRotationSpeed * Time.deltaTime;
                float MouseY = -Input.GetAxis("Mouse Y") * CameraRotationSpeed * Time.deltaTime;

                //Y angle calculation (90 to -90)
                float angleY = Vector3.Angle(new Vector3(transform.forward.x, 0, transform.forward.z), transform.forward);
                float dotY = Vector3.Dot(Vector3.up, transform.forward);
                if (dotY > 0) angleY = -angleY;

                //Clamp Y rotation
                float nextAngleY = angleY + MouseY;
                if (nextAngleY < CameraMinYRotation) MouseY = CameraMinYRotation - angleY;
                else if (nextAngleY > CameraMaxYRotation) MouseY = CameraMaxYRotation - angleY;

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

        void UpdateProbe()
        {
            if (Reflection)
            {
                Probe.transform.position = new Vector3(transform.position.x, -transform.position.y, transform.position.z);
                Probe.RenderProbe();
            }
        }

        void UpdateDepthOfField()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                float maxDistance = CameraMinZoom - CameraMaxZoom;
                float currentDistance = hit.distance - CameraMaxZoom;

                if (DepthOfField) FocusObject.transform.position = transform.position + (transform.forward * Mathf.Lerp(MinFocusDistance, MaxFocusDistance, currentDistance / maxDistance));

                //Fog
                if (Fog) RenderSettings.fogDensity = Mathf.SmoothDamp(RenderSettings.fogDensity, Mathf.Lerp(GameSettings.FogMaxDensity, GameSettings.FogMinDensity, currentDistance / maxDistance), ref FogVelocity, CameraFogSmoothness);
            }
        }

        //void UpdateBorders()
        //{
        //    if (Borders.Length != 0)
        //    {
        //        float FrontBorderInverse = Mathf.InverseLerp(GameSettings.MapDimension.y - BorderDistanceFadeThreshold, GameSettings.MapDimension.y - BorderMaxFadeThreshold, transform.position.z);
        //        float BackBorderInverse = Mathf.InverseLerp(-GameSettings.MapDimension.y + BorderDistanceFadeThreshold, -GameSettings.MapDimension.y + BorderMaxFadeThreshold, transform.position.z);
        //        float RightBorderInverse = Mathf.InverseLerp(GameSettings.MapDimension.x - BorderDistanceFadeThreshold, GameSettings.MapDimension.x - BorderMaxFadeThreshold, transform.position.x);
        //        float LeftBorderInverse = Mathf.InverseLerp(-GameSettings.MapDimension.x + BorderDistanceFadeThreshold, -GameSettings.MapDimension.x + BorderMaxFadeThreshold, transform.position.x);

        //        //Front fade
        //        if (FrontBorderInverse > 0)
        //        {
        //            BorderMaterials[0].color = new Color(BorderMaterials[0].color.r, BorderMaterials[0].color.g, BorderMaterials[0].color.b, Mathf.Lerp(0, BorderMaxAlpha, FrontBorderInverse));
        //            BorderMaterials[0].SetColor("_EmissionColor", Color.Lerp(Color.black, BorderMaxEmission, FrontBorderInverse));
        //        }
        //        else
        //        {
        //            if (BorderMaterials[0].color.a != 0)
        //            {
        //                BorderMaterials[0].color = new Color(BorderMaterials[0].color.r, BorderMaterials[0].color.g, BorderMaterials[0].color.b, 0);
        //                BorderMaterials[0].SetColor("_EmissionColor", Color.black);
        //            }
        //        }

        //        //Back fade
        //        if (BackBorderInverse > 0)
        //        {
        //            BorderMaterials[1].color = new Color(BorderMaterials[1].color.r, BorderMaterials[1].color.g, BorderMaterials[1].color.b, Mathf.Lerp(0, BorderMaxAlpha, BackBorderInverse));
        //        }
        //        else
        //        {
        //            if (BorderMaterials[1].color.a != 0)
        //            {
        //                BorderMaterials[1].color = new Color(BorderMaterials[1].color.r, BorderMaterials[1].color.g, BorderMaterials[1].color.b, 0);
        //            }
        //        }

        //        //Right fade
        //        if (RightBorderInverse > 0)
        //        {
        //            BorderMaterials[2].color = new Color(BorderMaterials[2].color.r, BorderMaterials[2].color.g, BorderMaterials[2].color.b, Mathf.Lerp(0, BorderMaxAlpha, RightBorderInverse));
        //        }
        //        else
        //        {
        //            if (BorderMaterials[2].color.a != 0)
        //            {
        //                BorderMaterials[2].color = new Color(BorderMaterials[2].color.r, BorderMaterials[2].color.g, BorderMaterials[2].color.b, 0);
        //            }
        //        }

        //        //Left fade
        //        if (LeftBorderInverse > 0)
        //        {
        //            BorderMaterials[3].color = new Color(BorderMaterials[3].color.r, BorderMaterials[3].color.g, BorderMaterials[3].color.b, Mathf.Lerp(0, BorderMaxAlpha, LeftBorderInverse));
        //        }
        //        else
        //        {
        //            if (BorderMaterials[3].color.a != 0)
        //            {
        //                BorderMaterials[3].color = new Color(BorderMaterials[3].color.r, BorderMaterials[3].color.g, BorderMaterials[3].color.b, 0);
        //            }
        //        }
        //    }
        //}
    }
}
