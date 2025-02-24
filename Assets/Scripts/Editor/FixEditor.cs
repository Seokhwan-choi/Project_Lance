using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lance
{
    [CustomEditor(typeof(Fix))]
    public class FixEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Fix fixResolution = (Fix)target;
            if (GUILayout.Button("Resolution"))
            {
                fixResolution.SetResolution();
            }
        }
    }
}
