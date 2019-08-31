using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using csDelaunay;

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
    public SerializedProperty MapMaskType;
    public SerializedProperty MapMaskRadius;
    public SerializedProperty MapLayers;
    //public SerializedProperty MapLayersColorThreshold;
    //public SerializedProperty MapLayersThresholdColors;
    public SerializedProperty MapLayersLayerThreshold;
    public SerializedProperty MapLayersMaxLayerHeight;

    //targeted script data
    private WorldGenerator TargetedScript;
    //Texture daata
    private Texture2D PreviewNormalGrayscaleTexture;
    private Texture2D PreviewVoronoiGrayscaleTexture;
    private Texture2D PreviewBaseLayerTexture;
    private Texture2D PreviewRegionMatchTexture;
    private Texture2D PreviewCombinedLayerTexture;
    //Voronoi data
    private List<MapRegion> MapRegions = new List<MapRegion>();
    private KDTree VoronoiKDTree;

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
        MapMaskType = serializedObject.FindProperty("MapMaskType");
        MapMaskRadius = serializedObject.FindProperty("MapMaskRadius");
        MapLayers = serializedObject.FindProperty("MapLayers");
        //MapLayersColorThreshold = MapLayers.FindPropertyRelative("ColorThreshold");
        //MapLayersThresholdColors = MapLayers.FindPropertyRelative("ThresholdColors");
        MapLayersLayerThreshold = MapLayers.FindPropertyRelative("_LayerThreshold");
        MapLayersMaxLayerHeight = MapLayers.FindPropertyRelative("MaxLayerHeight");
        GeneratePreviewMap();
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Total Map Dimension: " );
        EditorGUILayout.LabelField("Number of Chunks: " );
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            TargetedScript.MapLayers.LayerThreshold = TargetedScript.MapLayers._LayerThreshold;
            GeneratePreviewMap();
        }

        if (Event.current.type != EventType.ExecuteCommand)
        {
            DrawPreviewPicture("Normal Grayscale Preview", PreviewNormalGrayscaleTexture, 10);
            DrawPreviewPicture("Voronoi Grayscale Preview", PreviewVoronoiGrayscaleTexture, 10);
            DrawPreviewPicture("Normal Base Layer Preview", PreviewBaseLayerTexture, 10);
            DrawPreviewPicture("Region Match Preview", PreviewRegionMatchTexture, 10);
            DrawPreviewPicture("Combined Layer Preview", PreviewCombinedLayerTexture, 10);
        }
    }

    public void GeneratePreviewMap()
    {
        Random.InitState(MapSeed.stringValue.GetHashCode());

        //Normal map
        Vector2[] mapOctaveOffsets = TargetedScript.GetMapOctaveOffsets();

        //Voronoi map
        TargetedScript.GenerateVoronoiGraph(MapRegions);
        VoronoiKDTree = TargetedScript.CreateKDTree(MapRegions);
        TargetedScript.RandomizeMapRegions(ref MapRegions);

        //Create texture
        PreviewNormalGrayscaleTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);
        PreviewVoronoiGrayscaleTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);
        PreviewBaseLayerTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);
        PreviewRegionMatchTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);
        PreviewCombinedLayerTexture = new Texture2D(MapSize.vector2IntValue.x, MapSize.vector2IntValue.y);

        //Map center
        float centerX = (float)MapSize.vector2IntValue.x / 2;
        float centerY = (float)MapSize.vector2IntValue.y / 2;

        //Modify texture
        for (int x = 0; x < MapSize.vector2IntValue.x; x++)
        {
            for (int y = 0; y < MapSize.vector2IntValue.y; y++)
            {
                List<int> regionIndexes = new List<int>();
                float normalHeight = TargetedScript.GenerateMapCoordData(new Vector2(x - centerX, y - centerY), mapOctaveOffsets);
                float voronoiHeight = TargetedScript.GenerateVoronoiMapCoordData(new Vector2(x, y), MapRegions, VoronoiKDTree, regionIndexes);
                float voronoiMatchHeight = voronoiHeight;
                float combinedHeight = normalHeight * Mathf.Lerp(1, MapRegions[regionIndexes[0]].GenerationModifier, voronoiHeight);

                if (MapRegions[regionIndexes[0]].GenerationModifier == MapRegions[regionIndexes[1]].GenerationModifier)
                {
                    voronoiMatchHeight = 1;
                    combinedHeight = normalHeight * MapRegions[regionIndexes[0]].GenerationModifier;

                    if (MapRegions[regionIndexes[2]].GenerationModifier != MapRegions[regionIndexes[0]].GenerationModifier)
                    {
                        float closestDistance = Vector2.Distance(MapRegions[regionIndexes[0]].RegionSite.Coord, new Vector2(x, y));
                        float secondClosestDistance = Vector2.Distance(MapRegions[regionIndexes[1]].RegionSite.Coord, new Vector2(x, y));
                        float thirdClosestDistance = Vector2.Distance(MapRegions[regionIndexes[2]].RegionSite.Coord, new Vector2(x, y));
                        float matchBlend = 1 - (closestDistance / thirdClosestDistance);
                        voronoiMatchHeight = 0;
                        combinedHeight = normalHeight * Mathf.Lerp(1, MapRegions[regionIndexes[0]].GenerationModifier, matchBlend);
                    }
                }

                PreviewNormalGrayscaleTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, normalHeight));
                PreviewVoronoiGrayscaleTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, voronoiHeight));
                //PreviewSubtractiveLayerTexture.SetPixel(x, y, MapLayersThresholdColors.GetArrayElementAtIndex((int)MapLayersColorThreshold.animationCurveValue.Evaluate(height)).colorValue);
                PreviewBaseLayerTexture.SetPixel(x, y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(normalHeight) / MapLayersMaxLayerHeight.intValue));
                PreviewRegionMatchTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, voronoiMatchHeight));
                PreviewCombinedLayerTexture.SetPixel(x, y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(combinedHeight) / MapLayersMaxLayerHeight.intValue));
            }
        }

        PreviewNormalGrayscaleTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewNormalGrayscaleTexture.filterMode = FilterMode.Point;
        PreviewNormalGrayscaleTexture.Apply();

        PreviewVoronoiGrayscaleTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewVoronoiGrayscaleTexture.filterMode = FilterMode.Point;
        PreviewVoronoiGrayscaleTexture.Apply();

        PreviewBaseLayerTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewBaseLayerTexture.filterMode = FilterMode.Point;
        PreviewBaseLayerTexture.Apply();

        PreviewRegionMatchTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewRegionMatchTexture.filterMode = FilterMode.Point;
        PreviewRegionMatchTexture.Apply();

        PreviewCombinedLayerTexture.wrapMode = TextureWrapMode.Clamp;
        PreviewCombinedLayerTexture.filterMode = FilterMode.Point;
        PreviewCombinedLayerTexture.Apply();
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
