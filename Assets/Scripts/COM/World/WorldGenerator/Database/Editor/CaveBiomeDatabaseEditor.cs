using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace COM.Database.World
{
    [CustomEditor(typeof(CaveBiomeDatabase))]
    public class CaveBiomeDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CaveBiomeDatabase targetedScript = (CaveBiomeDatabase)target;

            DrawDefaultInspector();
            GUILayout.Label((targetedScript.CaveBiomes != null ? targetedScript.CaveBiomes.Count : 0) + " Registered Cave Biomes");

            EditorGUILayout.HelpBox("Removes everything", MessageType.Info);
            if (GUILayout.Button("Clear Database")) targetedScript.ClearDatabase();
        }
    }
}
