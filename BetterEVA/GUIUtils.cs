using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterEVA
{
    namespace GUIUtils
    {
        public class InputVector
        {
            readonly string[] axes_names = { "X", "Y", "Z" };

            public string prompt;

            Vector3 num_value;
            string[] text_value;

            public Vector3 Value
            {
                get
                {
                    return num_value;
                }
                set
                {
                    num_value = value;
                    text_value[0] = string.Format("{0:F6}", num_value.x);
                    text_value[1] = string.Format("{0:F6}", num_value.y);
                    text_value[2] = string.Format("{0:F6}", num_value.z);
                }
            }

            public InputVector(string prompt, Vector3 value)
            {
                this.prompt = prompt;

                text_value = new string[3];
                Value = value;
            }

            public void Draw()
            {
                GUILayout.BeginVertical();
                GUILayout.Label(prompt);

                for (int i = 0; i < 3; ++i)
                {
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(axes_names[i], GUILayout.MinWidth(10), GUILayout.MaxWidth(10));
                    text_value[i] = GUILayout.TextField(text_value[i]);
                    GUILayout.EndHorizontal();

                    if (float.TryParse(text_value[i], out float tmp))
                    {
                        num_value[i] = tmp;
                    }
                }

                GUILayout.EndVertical();
            }
        }
    }
}
