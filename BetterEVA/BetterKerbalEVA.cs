using UnityEngine;
using System.Collections;
using System.Reflection;

// TODO: limit camera rotation speed in space
// TODO: control yaw with mouse on ground
// TODO: command to face the target
// TODO: command to cancel relative velocity

namespace BetterEVA
{
    public class BetterKerbalEVA : PartModule
    {
        public enum CameraMode
        {
            GROUND,
            SPACE
        }

        EVACamera camera;
        KerbalEVA eva;
        static KSP.UI.Screens.Flight.NavBall navball;

        static readonly FieldInfo eva_tgtFwd = typeof(KerbalEVA).GetField("tgtFwd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        static readonly FieldInfo eva_tgtUp = typeof(KerbalEVA).GetField("tgtUp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        const float MinPitch = -1.0f;
        const float MaxPitch = 1.5f;


        [KSPField(guiName = "First person camera", guiActive = true, isPersistant = true), UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool FirstPerson = false;

        CameraMode mode = CameraMode.GROUND;

        public override void OnStart(StartState state)
        {
            camera = new EVACamera(vessel);
            GameEvents.onVesselSwitching.Add(this.OnVesselSwitching);
        }

        public void OnDestroy()
        {
            if (camera != null)
                camera.CameraActive = false;
        }

        public void OnVesselSwitching(Vessel from, Vessel to)
        {
            if (vessel == from)
                camera.CameraActive = false;
        }
        
        void Update()
        {
            if (vessel.isActiveVessel)
            {
                float scrollwheel = Input.GetAxis("Mouse ScrollWheel");
                if (scrollwheel != 0f)
                {
                    camera.maxDistance = camera.distance * Mathf.Exp(-scrollwheel);
                    camera.maxDistance = Mathf.Clamp(camera.maxDistance, 1.0f, 1000.0f);
                }

                float deltaPitch = 0;
                float deltaYaw = 0;
                if (Input.GetMouseButton(1)) // right-click
                {
                    deltaPitch = -0.1f * Input.GetAxis("Mouse Y");
                    deltaYaw = 0.1f * Input.GetAxis("Mouse X");
                }

                switch(vessel.situation)
                {
                    case Vessel.Situations.LANDED:
                    case Vessel.Situations.SPLASHED:
                    case Vessel.Situations.PRELAUNCH:
                    case Vessel.Situations.FLYING:
                        mode = CameraMode.GROUND;
                        camera.pitch = Mathf.Clamp(camera.pitch + deltaPitch, MinPitch, MaxPitch);
                        break;

                    case Vessel.Situations.SUB_ORBITAL:
                    case Vessel.Situations.ORBITING:
                    case Vessel.Situations.ESCAPING:
                        mode = CameraMode.SPACE;
                        camera.target_quaternion *= Quaternion.AngleAxis(deltaPitch * Mathf.Rad2Deg, Vector3.right);
                        camera.target_quaternion *= Quaternion.AngleAxis(deltaYaw * Mathf.Rad2Deg, Vector3.up);
                        break;

                    case Vessel.Situations.DOCKED:
                    default:
                        break;
                }
            }
        }

        void FixedUpdate()
        {
            if (eva == null)
                eva = part.FindModuleImplementing<KerbalEVA>();

            switch (mode)
            {
                case CameraMode.GROUND:
                    break;

                case CameraMode.SPACE:
                    Vector3 tgtFwd = camera.target_quaternion * Vector3.forward;
                    Vector3 tgtUp = camera.target_quaternion * Vector3.up;

                    GameSettings.EVA_ROTATE_ON_MOVE = false; // make sure the RCS don't fuck up the tgtFwd and tgtUp we're setting
                    eva_tgtFwd.SetValue(eva, tgtFwd);
                    eva_tgtUp.SetValue(eva, tgtUp);
                    break;
            }
        }

        int counter = 0;
        void LateUpdate()
        {
            if (vessel.isActiveVessel && counter < 2) // hack
            {
                counter++;
                camera.CameraActive = false;
            }
            else if (vessel.isActiveVessel && FirstPerson)
            {
                camera.CameraActive = true;
                switch (mode)
                {
                    case CameraMode.GROUND:
                        camera.UpdateCameraGround();
                        break;

                    case CameraMode.SPACE:
                        camera.UpdateCameraSpace();
                        break;
                }

                if (navball == null)
                    navball = FindObjectOfType<KSP.UI.Screens.Flight.NavBall>();

                System.Diagnostics.Debug.Assert(navball != null, "navball != null");

                CelestialBody mainBody = FlightGlobals.currentMainBody;

                if (mainBody == null || navball == null || navball.navBall == null) // apparently this can happen during vessel switching
                    return;

                Vector3 position = vessel.transform.position;

                navball.navBall.rotation = Quaternion.Inverse(camera.target_quaternion) * Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(
                        mainBody.position + (mainBody.transform.up * (float)mainBody.Radius) - position,
                        (position - mainBody.position).normalized
                        ).normalized,
                    (position - mainBody.position).normalized);
            }
            else
            {
                camera.CameraActive = false;
            }
        }
    }
}
