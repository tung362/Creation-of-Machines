using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using csDelaunay;
using COM;

namespace COM.Test
{
    [CustomEditor(typeof(WorldGeneratorComputeOld))]
    public class WorldGeneratorComputeEditor : Editor
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
        public SerializedProperty MapLayersLayerThreshold;
        public SerializedProperty MapLayersMaxLayerHeight;

        //targeted script data
        private WorldGeneratorComputeOld TargetedScript;
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
        private MapRegionGPU[] MapRegionGPUs;
        private KDTree VoronoiKDTree;
        //Chunk generation data
        private int CurrentChunkX = 0;
        private int CurrentChunkY = 0;
        private Vector2 Center = Vector2.zero;
        private Vector2[] MapOctaveOffsets;
        private Vector2[] SubMapOctaveOffsets;

        void OnEnable()
        {
            TargetedScript = (WorldGeneratorComputeOld)target;
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

            //GPU
            MapRegionGPUs = TargetedScript.CreateMapRegionGPUs(MapRegions);

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

            //for (int x = 0; x < MapSize.vector2IntValue.x; x++)
            //{
            //    for (int y = 0; y < MapSize.vector2IntValue.y; y++)
            //    {
            //        GeneratePreviewMapChunk(x, y);
            //    }
            //}
        }

        void GeneratePreviewMapChunk(int ChunkCoordX, int ChunkCoordY)
        {
            WGEditorOutputGPU[] normHeights = GenerateNoise(ChunkCoordX, ChunkCoordY, MapOctaveOffsets, SubMapOctaveOffsets, MapRegionGPUs);

            for (int i = 0; i < normHeights.Length; i++)
            {
                //Complete map
                Color caveColor = Color.red;
                float completeHeight = normHeights[i].combinedHeight;

                //If cave(subtractive) height is higher than surface(normal) height then it should punch a hole through the preview map
                if (completeHeight <= normHeights[i].subCombinedHeight)
                {
                    completeHeight = 1;
                    caveColor = Color.green;
                }

                PreviewNormGrayscaleTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].normHeight));
                PreviewVoronoiGrayscaleTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].voronoiHeight));
                PreviewBaseLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(normHeights[i].normHeight) / MapLayersMaxLayerHeight.intValue));

                PreviewRegionMatchTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].voronoiMatchHeight));
                PreviewCombinedLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(normHeights[i].combinedHeight) / MapLayersMaxLayerHeight.intValue));

                PreviewSubGrayscaleTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].subHeight));
                PreviewSubBaseLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(normHeights[i].subHeight) / MapLayersMaxLayerHeight.intValue));
                PreviewSubRegionMatchTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].subVoronoiMatchHeight));
                PreviewSubCombinedLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, MapLayersLayerThreshold.animationCurveValue.Evaluate(normHeights[i].subCombinedHeight) / MapLayersMaxLayerHeight.intValue));
                PreviewCompleteTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, caveColor, MapLayersLayerThreshold.animationCurveValue.Evaluate(completeHeight) / MapLayersMaxLayerHeight.intValue));
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

        public WGEditorOutputGPU[] GenerateNoise(int ChunkCoordX, int ChunkCoordY, Vector2[] mapOctaveOffsets, Vector2[] mapSubOctaveOffsets, MapRegionGPU[] mapRegionGPUs)
        {
            WGEditorOutputGPU[] outputs = new WGEditorOutputGPU[64];
            int threadNum = 8;

            int sizeOfOutputs;
            int sizeOfMapOctaveOffsets;
            int sizeOfMapRegionGPUs;
            unsafe
            {
                sizeOfOutputs = sizeof(WGEditorOutputGPU);
                sizeOfMapOctaveOffsets = sizeof(Vector2);
                sizeOfMapRegionGPUs = sizeof(MapRegionGPU);
            }

            int kernel = TargetedScript.WGEditorShader.FindKernel("NoiseEditorOld");

            ComputeBuffer resultBuffer = new ComputeBuffer(outputs.Length, sizeOfOutputs);
            resultBuffer.SetData(outputs);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "Result", resultBuffer);

            TargetedScript.WGEditorShader.SetInts("ThreadDimensions", new int[3] { threadNum, threadNum, 1 });
            TargetedScript.WGEditorShader.SetInts("ChunkCoord", new int[3] { ChunkCoordX, ChunkCoordY, 1 });
            TargetedScript.WGEditorShader.SetFloat("MapScale", MapScale.floatValue);
            TargetedScript.WGEditorShader.SetFloat("MapPersistance", MapPersistance.floatValue);
            TargetedScript.WGEditorShader.SetFloat("MapLacunarity", MapLacunarity.floatValue);
            TargetedScript.WGEditorShader.SetInt("Octaves", MapOctaves.intValue);
            TargetedScript.WGEditorShader.SetInt("Sites", mapRegionGPUs.Length);
            ComputeBuffer mapOctaveOffsetsBuffer = new ComputeBuffer(mapOctaveOffsets.Length, sizeOfMapOctaveOffsets);
            mapOctaveOffsetsBuffer.SetData(mapOctaveOffsets);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "MapOctaveOffsets", mapOctaveOffsetsBuffer);
            ComputeBuffer mapSubOctaveOffsetsBuffer = new ComputeBuffer(mapSubOctaveOffsets.Length, sizeOfMapOctaveOffsets);
            mapSubOctaveOffsetsBuffer.SetData(mapSubOctaveOffsets);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "SubMapOctaveOffsets", mapSubOctaveOffsetsBuffer);
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(mapRegionGPUs.Length, sizeOfMapRegionGPUs);
            mapRegionGPUsBuffer.SetData(mapRegionGPUs);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);

            TargetedScript.WGEditorShader.Dispatch(kernel, threadNum, threadNum, 1);

            resultBuffer.GetData(outputs);

            resultBuffer.Dispose();
            mapOctaveOffsetsBuffer.Dispose();
            mapRegionGPUsBuffer.Dispose();

            return outputs;
        }
    }
}
