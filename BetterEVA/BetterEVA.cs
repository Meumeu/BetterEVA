using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterEVA
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class BetterEVA : MonoBehaviour
    {
        //public static BetterEVA fetch;

        EVACamera camera;
        bool gameUIEnabled = true;
        Rect guiRect = new Rect(30, 30, 250, 400);
        float draggableHeight = 40;

        /*void Awake()
        {
            if (fetch)
            {
                Destroy(fetch);
            }

            fetch = this;
        }*/

        void Start()
        {
            camera = new EVACamera();
#if DEBUG
            input_local_pos = new GUIUtils.InputVector("Local position", camera.local_origin);
            input_local_dir = new GUIUtils.InputVector("Local direction", camera.local_direction);
#endif
            //GameSettings.EVA_Orient.primary.code = KeyCode.None;
            //GameSettings.EVA_ROTATE_ON_MOVE = false;
        }

        /*Vessel activeVessel;
        void FixedUpdate()
        {
            if (activeVessel != FlightGlobals.ActiveVessel)
            {
                if (activeVessel)
                    activeVessel.OnFlyByWire -= OnFlyByWire;

                activeVessel = FlightGlobals.ActiveVessel;

                if (activeVessel && activeVessel.isEVA)
                    activeVessel.OnFlyByWire += OnFlyByWire;
            }

            GameSettings.SAS_HOLD.GetKey();
        }

        void OnFlyByWire(FlightCtrlState state)
        {
            Debug.LogFormat("[BetterEVA] OnFlyByWire, rpy={0:F2},{1:F2},{2:F2}", state.roll, state.pitch, state.yaw);

            state.roll = 0;
            state.pitch = 0;
            state.yaw = 0;
        }*/

        void Update()
        {
            float scrollwheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollwheel != 0f)
            {
                camera.maxDistance = camera.maxDistance * Mathf.Exp(-scrollwheel);
                camera.maxDistance = Mathf.Clamp(camera.maxDistance, 1.0f, 100.0f);
            }
        }

        void LateUpdate()
        {
            Vessel vessel = FlightGlobals.ActiveVessel;

            camera.CameraActive = FlightGlobals.ready && vessel && vessel.isEVA;

            camera.UpdateCamera(vessel);
        }

#if DEBUG
        GUIUtils.InputVector input_local_pos;
        GUIUtils.InputVector input_local_dir;

        void GameUIEnable()
        {
            gameUIEnabled = true;
        }

        void GameUIDisable()
        {
            gameUIEnabled = false;
        }

        void OnGUI()
        {
            if (!gameUIEnabled)
                return;

            guiRect = GUI.Window(320, guiRect, GuiWindow, "BetterEVA");
        }
        
        void GuiWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, guiRect.width, draggableHeight));

            GUILayout.BeginVertical();
            input_local_pos.Draw();
            input_local_dir.Draw();
            GUILayout.Label("Situation: " + (FlightGlobals.ActiveVessel ? FlightGlobals.ActiveVessel.SituationString : ""));
            GUILayout.EndVertical();
            
            camera.local_direction = input_local_dir.Value;
            camera.local_origin = input_local_pos.Value;
        }
    }
#endif
}
