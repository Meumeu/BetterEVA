using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterEVA
{
    public class EVACamera
    {
        FlightCamera flightCamera;
        GameObject cameraParent;

        Vector3 origPosition;
        Quaternion origRotation;
        Transform origParent;
        float origNearClip;
        float origFoV;

        public float minDistance = 0.1f;
        public float maxDistance = 5.0f;

        public Vector3 local_origin = new Vector3(0.0f, 0.6f, -0.1f);
        public Vector3 local_direction = new Vector3(0.0f, 0.1f, -1.0f);

        public EVACamera()
        {
            flightCamera = FlightCamera.fetch;
            cameraParent = new GameObject("CameraParent");
        }

        void Save()
        {
            origPosition = flightCamera.transform.position;
            origRotation = flightCamera.transform.localRotation;
            origParent = flightCamera.transform.parent;
            origNearClip = Camera.main.nearClipPlane;
            origFoV = flightCamera.FieldOfView;
        }

        void Restore()
        {
            flightCamera.transform.parent = origParent;
            flightCamera.transform.position = origPosition;
            flightCamera.transform.rotation = origRotation;
            Camera.main.nearClipPlane = origNearClip;
            flightCamera.SetFoV(origFoV);
            flightCamera.ActivateUpdate();
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
                    Restore();
                }
                else if (!_cameraActive && value)
                {
                    Save();

                    flightCamera.SetTargetNone();
                    flightCamera.DeactivateUpdate();

                    
                }

                _cameraActive = value;
            }
        }

        public void UpdateCamera(Vessel v)
        {
            if (!CameraActive)
                return;

            Transform tr = flightCamera.transform;
            
            Vector3 origin = v.transform.TransformPoint(local_origin);

            Vector3 fwd = v.transform.TransformDirection(Vector3.forward);
            Vector3 up = (v.CoMD - v.mainBody.position).normalized;

            // Unity docs says "the Z axis will be aligned with forward/ and the Y axis with 
            // upwards if these vectors are orthogonal" but nothing if they are not:
            // we orthogonalize them ourselves to make sure they are correctly oriented
            fwd = (fwd - up * Vector3.Dot(fwd, up)).normalized;

            Quaternion q = (v.situation == Vessel.Situations.LANDED) ? Quaternion.LookRotation(fwd, up) : v.transform.rotation;

            Vector3 direction = q * local_direction;
            
            int layerMask = Physics.DefaultRaycastLayers;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, layerMask);

            float distance = maxDistance;
            foreach(RaycastHit hit in hits)
            {
                if (hit.distance > minDistance && hit.distance < distance)
                    distance = hit.distance;
            }
            
            tr.parent = cameraParent.transform;
            tr.SetPositionAndRotation(origin + distance * direction, q);
        }
    }
}
