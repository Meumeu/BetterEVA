using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


// TODO: smooth transitions

namespace BetterEVA
{
    public class EVACamera
    {
        FlightCamera flightCamera;
        GameObject cameraParent;
        Vessel vessel;
        
        public float minDistance = 0.1f;
        public float maxDistance = 5.0f;
        public float distance = 5.0f;

        public Vector3 local_origin = new Vector3(0.0f, 0.6f, -0.1f);

        public float pitch = 0.1f;
        public Quaternion target_quaternion = Quaternion.identity;

        public EVACamera(Vessel v)
        {
            flightCamera = FlightCamera.fetch;
            cameraParent = new GameObject("CameraParent");
            vessel = v;
        }

        bool _cameraActive = false;
        public bool CameraActive
        {
            get
            {
                return _cameraActive;
            }
            set
            {
                if (_cameraActive && !value)
                {
                    flightCamera.TargetActiveVessel();
                    flightCamera.ActivateUpdate();
                }
                else if (!_cameraActive && value)
                {
                    flightCamera.SetTargetNone();
                    flightCamera.DeactivateUpdate();
                }

                _cameraActive = value;
            }
        }

        public void UpdateCameraGround()
        {
            if (!CameraActive)
                return;
            
            Vector3 fwd = vessel.transform.TransformDirection(Vector3.forward);
            Vector3 up = (vessel.CoMD - vessel.mainBody.position).normalized;

            // Unity docs says "the Z axis will be aligned with forward/ and the Y axis with 
            // upwards if these vectors are orthogonal" but nothing if they are not:
            // we orthogonalize them ourselves to make sure they are correctly oriented
            fwd = (fwd - up * Vector3.Dot(fwd, up)).normalized;

            target_quaternion = Quaternion.LookRotation(fwd, up) * Quaternion.AngleAxis(pitch * Mathf.Rad2Deg, Vector3.right);
            
            UpdateCamera();
        }

        public void UpdateCameraSpace()
        {
            if (!CameraActive)
                return;

            UpdateCamera();
        }

        void UpdateCamera()
        {
            Transform tr = flightCamera.transform;
            
            Vector3 origin = vessel.transform.TransformPoint(local_origin);
            Vector3 direction = target_quaternion * Vector3.back;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, Physics.DefaultRaycastLayers);

            distance = maxDistance;
            foreach (RaycastHit hit in hits)
            {
                if (hit.distance > minDistance && hit.distance < distance)
                    distance = hit.distance;
            }

            tr.SetPositionAndRotation(origin + distance * direction, target_quaternion);

            
        }
    }
}
