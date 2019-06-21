using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CanEditMultipleObjects]
[CustomEditor(typeof(NavMeshRegion))]
public class NavMeshRegionEditor : Editor
{
    SerializedProperty TrackedObject;
    SerializedProperty BuildSize;
    SerializedProperty AgentID;
    SerializedProperty MinRegionArea;
    SerializedProperty ManualUpdate;

    void OnEnable()
    {
        TrackedObject = serializedObject.FindProperty("TrackedObject");
        BuildSize = serializedObject.FindProperty("BuildSize");
        AgentID = serializedObject.FindProperty("AgentID");
        MinRegionArea = serializedObject.FindProperty("MinRegionArea");
        ManualUpdate = serializedObject.FindProperty("ManualUpdate");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(TrackedObject);
        EditorGUILayout.PropertyField(BuildSize);
        UnityEditor.AI.NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", AgentID);
        EditorGUILayout.PropertyField(MinRegionArea);
        EditorGUILayout.PropertyField(ManualUpdate);

        serializedObject.ApplyModifiedProperties();
    }
}
