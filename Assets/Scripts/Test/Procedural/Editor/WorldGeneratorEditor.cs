using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using csDelaunay;

namespace COM.Test
{
    [CustomEditor(typeof(WorldGeneratorOld))]
    public class WorldGeneratorEditor : Editor
    {
        //Exposed properties
        public SerializedProperty MapSeed;
        public SerializedProperty MapSize;
        public SerializedProperty MapChunkSize;
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
        private WorldGeneratorOld TargetedScript;
        //Texture data
        private Texture2D PreviewNormGrayscaleTexture;
        private Texture2D PreviewVoronoiGrayscaleTexture;
        private Texture2D PreviewBaseLayerTexture;
        private Texture2D PreviewRegionMatchTexture;
        private Texture2D PreviewCombinedLayerTexture;
        private Texture2D PreviewSubGrayscaleTexture;
        private Texture2D PreviewSubBaseLayerTexture;
        private Texture2D PreviewSubRegionMatchTexture;
        private Texture2D PreviewSubCombinedLayerTexture;
        private Texture2D PreviewCompleteTexture;
        //Voronoi data
        private List<MapRegion> MapRegions = new List<MapRegion>();
        private KDTree VoronoiKDTree;
        //Chunk generation data
        private int CurrentChunkX = 0;
        private int CurrentChunkY = 0;
        private Vector2 Center = Vector2.zero;
        private Vector2[] MapOctaveOffsets;
        private Vector2[] SubMapOctaveOffsets;

        void OnEnable()
        {
            TargetedScript = (WorldGeneratorOld)target;
            MapSeed = serializedObject.FindProperty("MapSeed");
            MapSize = serializedObject.FindProperty("MapSize");
            MapChunkSize = serializedObject.FindProperty("MapChunkSize");
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
            EditorGUILayout.LabelField("Chunk Size: " + MapChunkSize.intValue + "x" + MapChunkSize.intValue);
            EditorGUILayout.LabelField("Total Map Dimension: " + MapSize.vector2IntValue.x * MapChunkSize.intValue + "x" + MapSize.vector2IntValue.y * MapChunkSize.intValue);
            EditorGUILayout.LabelField("Number of Chunks: " + MapSize.vector2IntValue.x * MapSize.vector2IntValue.y);
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                TargetedScript.MapLayers.LayerThreshold = TargetedScript.MapLayers._LayerThreshold;
                GeneratePreviewMap();
            }

            if (Event.current.type != EventType.ExecuteCommand)
            {
                DrawPreviewPicture("Normal Grayscale Preview", PreviewNormGrayscaleTexture, 10);
                DrawPreviewPicture("Voronoi Grayscale Preview", PreviewVoronoiGrayscaleTexture, 10);
                DrawPreviewPicture("Normal Base Layer Preview", PreviewBaseLayerTexture, 10);
                DrawPreviewPicture("Region Match Preview", PreviewRegionMatchTexture, 10);
                DrawPreviewPicture("Combined Layer Preview", PreviewCombinedLayerTexture, 10);
                DrawPreviewPicture("Subtractive Grayscale Preview", PreviewSubGrayscaleTexture, 10);
                DrawPreviewPicture("Subtractive Base Layer Preview", PreviewSubBaseLayerTexture, 10);
                DrawPreviewPicture("Subtractive Region Match Preview", PreviewSubRegionMatchTexture, 10);
                DrawPreviewPicture("Subtractive Combined Layer Preview", PreviewSubCombinedLayerTexture, 10);
                DrawPreviewPicture("Complete Preview", PreviewCompleteTexture, 10);
            }

            //Creates 1 chunk every frame to reduce lag
            if (CurrentChunkX < MapSize.vector2IntValue.x)
            {
                GeneratePreviewMapChunk(CurrentChunkX, CurrentChunkY);
                CurrentChunkY++;
                if (CurrentChunkY >= MapSize.vector2IntValue.y)
                {
                    CurrentChunkY = 0;
                    CurrentChunkX++;
                }
                Repaint();
            }
        }

        public void GeneratePreviewMap()
        {
            CurrentChunkX = 0;
            CurrentChunkY = 0;

            //Voronoi map
            TargetedScript.GenerateVoronoiGraph(MapRegions);
            VoronoiKDTree = TargetedScript.CreateKDTree(MapRegions);

            //Normal map
            MapOctaveOffsets = TargetedScript.GenerateMapOctaveOffsets(false);
            TargetedScript.RandomizeMapRegions(MapRegions, false);

            //Subtractive map
            SubMapOctaveOffsets = TargetedScript.GenerateMapOctaveOffsets(true);
            TargetedScript.RandomizeMapRegions(MapRegions, true);

            //Create texture
            PreviewNormGrayscaleTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewVoronoiGrayscaleTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewBaseLayerTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewRegionMatchTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewCombinedLayerTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewSubGrayscaleTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewSubBaseLayerTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewSubRegionMatchTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewSubCombinedLayerTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);
            PreviewCompleteTexture = new Texture2D(MapSize.vector2IntValue.x * MapChunkSize.intValue, MapSize.vector2IntValue.y * MapChunkSize.intValue);

            //Map center
            Center = new Vector2((float)MapSize.vector2IntValue.x / 2, (float)MapSize.vector2IntValue.y / 2);

            PreviewNormGrayscaleTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewNormGrayscaleTexture.filterMode = FilterMode.Point;
            PreviewNormGrayscaleTexture.Apply();

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

            PreviewSubGrayscaleTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewSubGrayscaleTexture.filterMode = FilterMode.Point;
            PreviewSubGrayscaleTexture.Apply();

            PreviewSubBaseLayerTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewSubBaseLayerTexture.filterMode = FilterMode.Point;
            PreviewSubBaseLayerTexture.Apply();

            PreviewSubRegionMatchTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewSubRegionMatchTexture.filterMode = FilterMode.Point;
            PreviewSubRegionMatchTexture.Apply();

            PreviewSubCombinedLayerTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewSubCombinedLayerTexture.filterMode = FilterMode.Point;
            PreviewSubCombinedLayerTexture.Apply();

            PreviewCompleteTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewCompleteTexture.filterMode = FilterMode.Point;
            PreviewCompleteTexture.Apply();
        }

        void GeneratePreviewMapChunk(int ChunkCoordX, int ChunkCoordY)
        {
            //Modify texture
            for (int x = ChunkCoordX * MapChunkSize.intValue; x < (ChunkCoordX * MapChunkSize.intValue) + MapChunkSize.intValue; x++)
            {
                for (int y = ChunkCoordY * MapChunkSize.intValue; y < (ChunkCoordY * MapChunkSize.intValue) + MapChunkSize.intValue; y++)
                {
                    //Voronoi map
                    List<int> regionIndexes = new List<int>();
                    float voronoiHeight = TargetedScript.GenerateVoronoiMapCoordData(new Vector2(x, y), MapRegions, VoronoiKDTree, regionIndexes);

                    //Normal map
                    float normHeight = TargetedScript.GenerateMapCoordData(new Vector2(x - Center.x, y - Center.y), MapOctaveOffsets);
                    float voronoiMatchHeight = voronoiHeight;
                    //float combinedHeight = normHeight * Mathf.Lerp(1, MapRegions[regionIndexes[0]].GenerationModifier, voronoiHeight);
                    float combinedHeight = TargetedScript.BiomeHeightBlend(normHeight, voronoiHeight, new Vector2(x, y), regionIndexes, MapRegions, false);

                    //Subtractive map
                    float subHeight = TargetedScript.GenerateMapCoordData(new Vector2(x - Center.x, y - Center.y), SubMapOctaveOffsets);
                    float subVoronoiMatchHeight = voronoiHeight;
                    float subCombinedHeight = TargetedScript.BiomeHeightBlend(subHeight, voronoiHeight, new Vector2(x, y), regionIndexes, MapRegions, true);

                    //Complete map
                    float completeHeight = combinedHeight;
                    Color caveColor = Color.red;

                    //If cave(subtractive) height is higher than surface(normal) height then it should punch a hole through the preview map
                    if (combinedHeight <= subCombinedHeight)
                    {
                        completeHeight = 1;
                        caveColor = Color.green;
                    }

                    PreviewNormGrayscaleTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, normHeight));
                    PreviewVoronoiGrayscaleTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, voronoiHeight));
                    //PreviewSubtractiveLayerTexture.SetPixel(x, y, MapLayersThresholdColors.GetArrayElementAtIndex((int)MapLayersColorThreshold.animationCurveValue.Evaluate(height)).colorValue);
                    PreviewBaseLayerTexture.SetPixel(x, y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(normHeight) / MapLayersMaxLayerHeight.intValue));
                    PreviewRegionMatchTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, voronoiMatchHeight));
                    PreviewCombinedLayerTexture.SetPixel(x, y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(combinedHeight) / MapLayersMaxLayerHeight.intValue));

                    PreviewSubGrayscaleTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, subHeight));
                    PreviewSubBaseLayerTexture.SetPixel(x, y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(subHeight) / MapLayersMaxLayerHeight.intValue));
                    PreviewSubRegionMatchTexture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, subVoronoiMatchHeight));
                    PreviewSubCombinedLayerTexture.SetPixel(x, y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(subCombinedHeight) / MapLayersMaxLayerHeight.intValue));
                    PreviewCompleteTexture.SetPixel(x, y, Color.Lerp(Color.blue, caveColor, MapLayersLayerThreshold.animationCurveValue.Evaluate(completeHeight) / MapLayersMaxLayerHeight.intValue));
                }
            }

            PreviewNormGrayscaleTexture.Apply();
            PreviewVoronoiGrayscaleTexture.Apply();
            PreviewBaseLayerTexture.Apply();
            PreviewRegionMatchTexture.Apply();
            PreviewCombinedLayerTexture.Apply();
            PreviewSubGrayscaleTexture.Apply();
            PreviewSubBaseLayerTexture.Apply();
            PreviewSubRegionMatchTexture.Apply();
            PreviewSubCombinedLayerTexture.Apply();
            PreviewCompleteTexture.Apply();
        }

        void DrawPreviewPicture(string lableName, Texture2D objectReferenceValue, float spacing)
        {
            if (objectReferenceValue)
            {
                EditorGUILayout.LabelField(lableName, EditorStyles.boldLabel);
                EditorGUI.DrawPreviewTexture(new Rect(EditorGUILayout.GetControlRect().x, EditorGUILayout.GetControlRect().y - 20, EditorGUILayout.GetControlRect().width, EditorGUILayout.GetControlRect().width), objectReferenceValue);
                GUILayout.Space(EditorGUIUtility.currentViewWidth - 100);
                GUILayout.Space(spacing);
            }
        }
    }
}
