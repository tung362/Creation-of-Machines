using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace COM
{
    [CustomEditor(typeof(WorldGenerator))]
    public class WorldGeneratorEditor : Editor
    {
        //targeted script data
        private WorldGenerator TargetedScript;

        //Settings
        public static Vector2Int PreviewSize = new Vector2Int(152, 152);

        //Texture data
        private Texture2D PreviewVoronoiGrayscaleTexture;
        private Texture2D PreviewRegionMatchTexture;

        //Voronoi data
        private List<Region> Regions = new List<Region>();
        private RegionGPU[] RegionGPUs;

        //Chunk generation data
        private int CurrentChunkX = 0;
        private int CurrentChunkY = 0;

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
            PreviewSize = EditorGUILayout.Vector2IntField("Preview Size", PreviewSize);
            if (EditorGUI.EndChangeCheck()) GeneratePreviewMap();

            if (Event.current.type != EventType.ExecuteCommand)
            {
                DrawPreviewPicture("Voronoi Grayscale Preview", PreviewVoronoiGrayscaleTexture, 10);
                DrawPreviewPicture("Region Match Preview", PreviewRegionMatchTexture, 10);
            }

            //Creates 1 chunk every frame to reduce lag
            if (CurrentChunkX < Mathf.CeilToInt(PreviewSize.x / 8.0f))
            {
                GeneratePreviewMapChunk(CurrentChunkX, CurrentChunkY);
                CurrentChunkY++;
                if (CurrentChunkY >= Mathf.CeilToInt(PreviewSize.y / 8.0f))
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
            TargetedScript.GenerateVoronoiGraph(PreviewSize, Regions);

            //Normal map
            TargetedScript.RandomizeMapRegions(Regions);

            //GPU
            RegionGPUs = TargetedScript.CreateRegionGPUs(Regions);

            //Create texture
            PreviewVoronoiGrayscaleTexture = new Texture2D(PreviewSize.x, PreviewSize.y);
            PreviewRegionMatchTexture = new Texture2D(PreviewSize.x, PreviewSize.y);

            PreviewVoronoiGrayscaleTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewVoronoiGrayscaleTexture.filterMode = FilterMode.Point;
            PreviewVoronoiGrayscaleTexture.Apply();

            PreviewRegionMatchTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewRegionMatchTexture.filterMode = FilterMode.Point;
            PreviewRegionMatchTexture.Apply();

            //for (int x = 0; x < Mathf.CeilToInt(PreviewSize.x / 8.0f); x++)
            //{
            //    for (int y = 0; y < Mathf.CeilToInt(PreviewSize.y / 8.0f); y++)
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
            WGEditorOutputGPU[] normHeights = GenerateNoise(ChunkCoordX, ChunkCoordY, RegionGPUs);

            for (int i = 0; i < normHeights.Length; i++)
            {
                //Complete map
                Color caveColor = Color.red;

                PreviewVoronoiGrayscaleTexture.SetPixel(normHeights[i].Coord.x, normHeights[i].Coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].VoronoiHeight));
                PreviewRegionMatchTexture.SetPixel(normHeights[i].Coord.x, normHeights[i].Coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].VoronoiMatchHeight));
            }

            PreviewVoronoiGrayscaleTexture.Apply();
            PreviewRegionMatchTexture.Apply();
        }

        public WGEditorOutputGPU[] GenerateNoise(int ChunkCoordX, int ChunkCoordY, RegionGPU[] mapRegionGPUs)
        {
            //Run through a chunk of 8x8x8 grid tiles
            int threadNum = 8;

            //Outputs a Chunk of 8x8 pixels = 64
            WGEditorOutputGPU[] heightOutputs = new WGEditorOutputGPU[64];

            //Find bytes
            int sizeOfPixelOutputs;
            int sizeOfMapRegionGPUs;
            unsafe
            {
                sizeOfPixelOutputs = sizeof(WGEditorOutputGPU);
                sizeOfMapRegionGPUs = sizeof(RegionGPU);
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
            TargetedScript.WGEditorShader.SetInt("Sites", mapRegionGPUs.Length);

            //StructuredBuffer sets
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(mapRegionGPUs.Length, sizeOfMapRegionGPUs);
            mapRegionGPUsBuffer.SetData(mapRegionGPUs);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);

            //Run kernal
            TargetedScript.WGEditorShader.Dispatch(kernel, threadNum, threadNum, 1);

            //Grab outputs
            heightBuffer.GetData(heightOutputs);

            //Get rid of buffer data
            heightBuffer.Dispose();
            mapRegionGPUsBuffer.Dispose();

            return heightOutputs;
        }
    }
}
