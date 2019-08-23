using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGenerator))]
public class WorldGeneratorEditor : Editor
{
    //Exposed properties
    public SerializedProperty MapSeed;
    public SerializedProperty MapSize;
    public SerializedProperty MapScale;
    public SerializedProperty MapOctaves;
    public SerializedProperty MapPersistance;
    public SerializedProperty MapLacunarity;
    public SerializedProperty MapOffset;
    public SerializedProperty GradientType;
    public SerializedProperty GradientRadius;

    //Data
    private WorldGenerator TargetedScript;
    private Texture2D PreviewBaseLayerTexture;
    private Texture2D PreviewSubtractiveLayerTexture;
    private Texture2D PreviewCompleteLayerTexture;

    void OnEnable()
    {
        TargetedScript = (WorldGenerator)target;
        MapSeed = serializedObject.FindProperty("MapSeed");
        MapSize = serializedObject.FindProperty("MapSize");
        MapScale = serializedObject.FindProperty("MapScale");
        MapOctaves = serializedObject.FindProperty("MapOctaves");
        MapPersistance = serializedObject.FindProperty("MapPersistance");
        MapLacunarity = serializedObject.FindProperty("MapLacunarity");
        MapOffset = serializedObject.FindProperty("MapOffset");
        GradientType = serializedObject.FindProperty("GradientType");
        GradientRadius = serializedObject.FindProperty("GradientRadius");
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck()) GeneratePreviewMap();

        DrawPreviewPicture("Base Layer Preview", PreviewBaseLayerTexture, 10);
        DrawPreviewPicture("Subtractive Layer Preview", PreviewSubtractiveLayerTexture, 10);
        DrawPreviewPicture("Complete Preview", PreviewCompleteLayerTexture, 10);
    }

    public void GeneratePreviewMap()
    {
        Random.InitState(MapSeed.stringValue.GetHashCode());

        Vector2[] mapOctaveOffsets = TargetedScript.GetMapOctaveOffsets();

        PreviewBaseLayerTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);
        PreviewSubtractiveLayerTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);
        PreviewCompleteLayerTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);

        float centerX = (float)MapSize.vector2IntValue.x / 2;
        float centerY = (float)MapSize.vector2IntValue.y / 2;

        for (int x = 0; x < MapSize.vector2IntValue.x; x++)
        {
            for (int y = 0; y < MapSize.vector2IntValue.y; y++)
            {
                PreviewBaseLayerTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, TargetedScript.GenerateMapCoordData(new Vector2(x - centerX, y - centerY), mapOctaveOffsets)));
            }
        }

        PreviewBaseLayerTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewBaseLayerTexture.filterMode = FilterMode.Point;
        PreviewBaseLayerTexture.Apply();

        PreviewSubtractiveLayerTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewSubtractiveLayerTexture.filterMode = FilterMode.Point;
        PreviewSubtractiveLayerTexture.Apply();

        PreviewCompleteLayerTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewCompleteLayerTexture.filterMode = FilterMode.Point;
        PreviewCompleteLayerTexture.Apply();
    }

    void DrawPreviewPicture(string lableName, Texture2D objectReferenceValue, float spacing)
    {
        if(objectReferenceValue)
        {
            EditorGUILayout.LabelField(lableName);
            EditorGUI.DrawPreviewTexture(new Rect(EditorGUILayout.GetControlRect().x, EditorGUILayout.GetControlRect().y - 20, EditorGUILayout.GetControlRect().width, EditorGUILayout.GetControlRect().width), objectReferenceValue);
            GUILayout.Space(EditorGUIUtility.currentViewWidth - 100);
            GUILayout.Space(spacing);
        }
    }
}
