using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CanEditMultipleObjects]
[CustomEditor(typeof(NavMeshRegionTag))]
public class NavMeshRegionTagEditor : Editor
{
    SerializedProperty BuildType;
    SerializedProperty AreaID;

    void OnEnable()
    {
        BuildType = serializedObject.FindProperty("BuildType");
        AreaID = serializedObject.FindProperty("AreaID");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(BuildType);
        UnityEditor.AI.NavMeshComponentsGUIUtility.AreaPopup("Area Type", AreaID);

        serializedObject.ApplyModifiedProperties();
    }
}
