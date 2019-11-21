using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace COM.Test2
{
    [CustomEditor(typeof(WorldGenerator))]
    public class WorldGeneratorEditor : Editor
    {
        //targeted script data
        private WorldGenerator TargetedScript;

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

        //Chunk generation data
        private int CurrentChunkX = 0;
        private int CurrentChunkY = 0;
        private Vector2[] MapOctaveOffsets;
        private Vector2[] SubMapOctaveOffsets;

        void OnEnable()
        {
            TargetedScript = (WorldGenerator)target;
            GeneratePreviewMap();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Map Size: " + TargetedScript.MapSize.x + "x" + TargetedScript.MapSize.y);
            EditorGUILayout.LabelField("Number of Chunks: " + Mathf.CeilToInt((TargetedScript.MapSize.x / TargetedScript.ChunkSize) * (TargetedScript.MapSize.y / TargetedScript.ChunkSize)));
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
            if (CurrentChunkX < Mathf.CeilToInt(TargetedScript.MapSize.x / 8.0f))
            {
                GeneratePreviewMapChunk(CurrentChunkX, CurrentChunkY);
                CurrentChunkY++;
                if (CurrentChunkY >= Mathf.CeilToInt(TargetedScript.MapSize.y / 8.0f))
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

            //Normal map
            MapOctaveOffsets = TargetedScript.GenerateMapOctaveOffsets(false);
            TargetedScript.RandomizeMapRegions(MapRegions, false);

            //Subtractive map
            SubMapOctaveOffsets = TargetedScript.GenerateMapOctaveOffsets(true);
            TargetedScript.RandomizeMapRegions(MapRegions, true);

            //GPU
            MapRegionGPUs = TargetedScript.CreateMapRegionGPUs(MapRegions);

            //Create texture
            PreviewNormGrayscaleTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewVoronoiGrayscaleTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewBaseLayerTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewRegionMatchTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewCombinedLayerTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewSubGrayscaleTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewSubBaseLayerTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewSubRegionMatchTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewSubCombinedLayerTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewCompleteTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);

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

            //for (int x = 0; x < Mathf.CeilToInt(TargetedScript.MapSize.x / 8.0f); x++)
            //{
            //    for (int y = 0; y < Mathf.CeilToInt(TargetedScript.MapSize.y / 8.0f); y++)
            //    {
            //        GeneratePreviewMapChunk(x, y);
            //    }
            //}
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
                PreviewBaseLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, TargetedScript.MapLayers._LayerThreshold.Evaluate(normHeights[i].normHeight) / TargetedScript.MapLayers.MaxLayerHeight));

                PreviewRegionMatchTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].voronoiMatchHeight));
                PreviewCombinedLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, TargetedScript.MapLayers._LayerThreshold.Evaluate(normHeights[i].combinedHeight) / TargetedScript.MapLayers.MaxLayerHeight));

                PreviewSubGrayscaleTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].subHeight));
                PreviewSubBaseLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, TargetedScript.MapLayers._LayerThreshold.Evaluate(normHeights[i].subHeight) / TargetedScript.MapLayers.MaxLayerHeight));
                PreviewSubRegionMatchTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].subVoronoiMatchHeight));
                PreviewSubCombinedLayerTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, Color.red, TargetedScript.MapLayers._LayerThreshold.Evaluate(normHeights[i].subCombinedHeight) / TargetedScript.MapLayers.MaxLayerHeight));
                PreviewCompleteTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.blue, caveColor, TargetedScript.MapLayers._LayerThreshold.Evaluate(completeHeight) / TargetedScript.MapLayers.MaxLayerHeight));
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

        public WGEditorOutputGPU[] GenerateNoise(int ChunkCoordX, int ChunkCoordY, Vector2[] mapOctaveOffsets, Vector2[] mapSubOctaveOffsets, MapRegionGPU[] mapRegionGPUs)
        {
            //Run through a chunk of 8x8x8 grid tiles
            int threadNum = 8;

            //Outputs a Chunk of 8x8 pixels = 64
            WGEditorOutputGPU[] heightOutputs = new WGEditorOutputGPU[64];

            //Find bytes
            int sizeOfPixelOutputs;
            int sizeOfMapOctaveOffsets;
            int sizeOfMapRegionGPUs;
            unsafe
            {
                sizeOfPixelOutputs = sizeof(WGEditorOutputGPU);
                sizeOfMapOctaveOffsets = sizeof(Vector2);
                sizeOfMapRegionGPUs = sizeof(MapRegionGPU);
            }

            //Kernal
            int kernel = TargetedScript.WGEditorShader.FindKernel("NoiseEditor");

            //RWStructuredBuffer sets
            ComputeBuffer heightBuffer = new ComputeBuffer(heightOutputs.Length, sizeOfPixelOutputs);
            heightBuffer.SetData(heightOutputs);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "Heights", heightBuffer);

            //Sets
            TargetedScript.WGEditorShader.SetInts("ThreadDimensions", new int[3] { threadNum, threadNum, 1 });
            TargetedScript.WGEditorShader.SetInts("ChunkCoord", new int[3] { ChunkCoordX, ChunkCoordY, 1 });
            TargetedScript.WGEditorShader.SetFloat("MapScale", TargetedScript.MapScale);
            TargetedScript.WGEditorShader.SetFloat("MapPersistance", TargetedScript.MapPersistance);
            TargetedScript.WGEditorShader.SetFloat("MapLacunarity", TargetedScript.MapLacunarity);
            TargetedScript.WGEditorShader.SetInt("Octaves", TargetedScript.MapOctaves);
            TargetedScript.WGEditorShader.SetInt("Sites", mapRegionGPUs.Length);

            //StructuredBuffer sets
            ComputeBuffer mapOctaveOffsetsBuffer = new ComputeBuffer(mapOctaveOffsets.Length, sizeOfMapOctaveOffsets);
            ComputeBuffer mapSubOctaveOffsetsBuffer = new ComputeBuffer(mapSubOctaveOffsets.Length, sizeOfMapOctaveOffsets);
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(mapRegionGPUs.Length, sizeOfMapRegionGPUs);
            mapOctaveOffsetsBuffer.SetData(mapOctaveOffsets);
            mapSubOctaveOffsetsBuffer.SetData(mapSubOctaveOffsets);
            mapRegionGPUsBuffer.SetData(mapRegionGPUs);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "MapOctaveOffsets", mapOctaveOffsetsBuffer);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "SubMapOctaveOffsets", mapSubOctaveOffsetsBuffer);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);

            //Run kernal
            TargetedScript.WGEditorShader.Dispatch(kernel, threadNum, threadNum, 1);

            //Grab outputs
            heightBuffer.GetData(heightOutputs);

            //Get rid of buffer data
            heightBuffer.Dispose();
            mapOctaveOffsetsBuffer.Dispose();
            mapSubOctaveOffsetsBuffer.Dispose();
            mapRegionGPUsBuffer.Dispose();

            return heightOutputs;
        }
    }
}
