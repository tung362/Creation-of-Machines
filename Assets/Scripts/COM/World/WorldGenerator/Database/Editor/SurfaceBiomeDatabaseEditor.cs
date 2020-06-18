using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace COM.Database.World
{
    [CustomEditor(typeof(SurfaceBiomeDatabase))]
    public class SurfaceBiomeDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SurfaceBiomeDatabase targetedScript = (SurfaceBiomeDatabase)target;

            DrawDefaultInspector();
            GUILayout.Label((targetedScript.SurfaceBiomes != null ? targetedScript.SurfaceBiomes.Count : 0) + " Registered Surface Biomes");

            EditorGUILayout.HelpBox("Removes everything", MessageType.Info);
            if (GUILayout.Button("Clear Database")) targetedScript.ClearDatabase();
        }
    }
}
