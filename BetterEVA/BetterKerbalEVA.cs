using UnityEngine;
using System.Collections;
using System.Reflection;

// TODO: Reset camera when switching vessels
// TODO: limit camera rotation speed in space
// TODO: control yaw with mouse on ground
// TODO: update the navball

namespace BetterEVA
{
    public class BetterKerbalEVA : PartModule
    {
        EVACamera camera;
        KerbalEVA eva;

        static readonly FieldInfo eva_tgtFwd = typeof(KerbalEVA).GetField("tgtFwd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        static readonly FieldInfo eva_tgtUp = typeof(KerbalEVA).GetField("tgtUp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);


        const float MinPitch = -1.0f;
        const float MaxPitch = 1.5f;


        [KSPField(guiName = "First person camera", guiActive = true, isPersistant = true), UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool FirstPerson = false;

        public override void OnStart(StartState state)
        {
            camera = new EVACamera(vessel);
        }

        public void OnDestroy()
        {
            FirstPerson = false;
            camera.CameraActive = false;
        }

        public enum CameraMode
        {
            GROUND,
            SPACE
        }

        CameraMode mode = CameraMode.GROUND;

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
            {
                eva = part.FindModuleImplementing<KerbalEVA>();
            }

            switch (mode)
            {
                case CameraMode.GROUND:
                    break;

                case CameraMode.SPACE:
                    Vector3 tgtFwd = camera.target_quaternion * Vector3.forward;
                    Vector3 tgtUp = camera.target_quaternion * Vector3.up;

                    GameSettings.EVA_ROTATE_ON_MOVE = false;
                    eva_tgtFwd.SetValue(eva, tgtFwd);
                    eva_tgtUp.SetValue(eva, tgtUp);
                    break;
            }
        }

        int counter = 0;
        void LateUpdate()
        {
            if (vessel.isActiveVessel)
            {
                System.Diagnostics.Debug.Assert(FlightGlobals.ActiveVessel.isEVA, "FlightGlobals.ActiveVessel.isEVA");

                if (counter < 2) // hack
                {
                    counter++;
                    camera.CameraActive = false;
                }
                else
                {
                    camera.CameraActive = FirstPerson;
                    switch (mode)
                    {
                        case CameraMode.GROUND:
                            camera.UpdateCameraGround();
                            break;

                        case CameraMode.SPACE:
                            camera.UpdateCameraSpace();
                            break;
                    }
                }
            }
            else
            {
                camera.CameraActive = false;
            }
        }
    }
}
