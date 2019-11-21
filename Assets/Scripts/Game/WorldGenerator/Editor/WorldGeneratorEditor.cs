﻿using System.Collections;
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

        //Texture data
        private Texture2D PreviewVoronoiGrayscaleTexture;
        private Texture2D PreviewRegionMatchTexture;

        //Voronoi data
        private List<MapRegion> MapRegions = new List<MapRegion>();
        private MapRegionGPU[] MapRegionGPUs;

        //Chunk generation data
        private int CurrentChunkX = 0;
        private int CurrentChunkY = 0;
        private Vector3[] MapOctaveOffsets;

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
                GeneratePreviewMap();
            }

            if (Event.current.type != EventType.ExecuteCommand)
            {
                DrawPreviewPicture("Voronoi Grayscale Preview", PreviewVoronoiGrayscaleTexture, 10);
                DrawPreviewPicture("Region Match Preview", PreviewRegionMatchTexture, 10);
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
            TargetedScript.RandomizeMapRegions(MapRegions, true);

            //GPU
            MapRegionGPUs = TargetedScript.CreateMapRegionGPUs(MapRegions);

            //Create texture
            PreviewVoronoiGrayscaleTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);
            PreviewRegionMatchTexture = new Texture2D(TargetedScript.MapSize.x, TargetedScript.MapSize.y);

            PreviewVoronoiGrayscaleTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewVoronoiGrayscaleTexture.filterMode = FilterMode.Point;
            PreviewVoronoiGrayscaleTexture.Apply();

            PreviewRegionMatchTexture.wrapMode = TextureWrapMode.Clamp;
            PreviewRegionMatchTexture.filterMode = FilterMode.Point;
            PreviewRegionMatchTexture.Apply();

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
            WGEditorOutputGPU[] normHeights = GenerateNoise(ChunkCoordX, ChunkCoordY, MapOctaveOffsets, MapRegionGPUs);

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

                PreviewVoronoiGrayscaleTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].voronoiHeight));
                PreviewRegionMatchTexture.SetPixel(normHeights[i].coord.x, normHeights[i].coord.y, Color.Lerp(Color.black, Color.white, normHeights[i].voronoiMatchHeight));
            }

            PreviewVoronoiGrayscaleTexture.Apply();
            PreviewRegionMatchTexture.Apply();
        }

        public WGEditorOutputGPU[] GenerateNoise(int ChunkCoordX, int ChunkCoordY, Vector3[] mapOctaveOffsets, MapRegionGPU[] mapRegionGPUs)
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
                sizeOfMapOctaveOffsets = sizeof(Vector3);
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
            TargetedScript.WGEditorShader.SetFloat("MapScale", TargetedScript.SurfaceScale);
            TargetedScript.WGEditorShader.SetFloat("MapPersistance", TargetedScript.SurfacePersistance);
            TargetedScript.WGEditorShader.SetFloat("MapLacunarity", TargetedScript.SurfaceLacunarity);
            TargetedScript.WGEditorShader.SetInt("Octaves", TargetedScript.SurfaceOctaves);
            TargetedScript.WGEditorShader.SetInt("Sites", mapRegionGPUs.Length);

            //StructuredBuffer sets
            ComputeBuffer mapOctaveOffsetsBuffer = new ComputeBuffer(mapOctaveOffsets.Length, sizeOfMapOctaveOffsets);
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(mapRegionGPUs.Length, sizeOfMapRegionGPUs);
            mapOctaveOffsetsBuffer.SetData(mapOctaveOffsets);
            mapRegionGPUsBuffer.SetData(mapRegionGPUs);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "MapOctaveOffsets", mapOctaveOffsetsBuffer);
            TargetedScript.WGEditorShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);

            //Run kernal
            TargetedScript.WGEditorShader.Dispatch(kernel, threadNum, threadNum, 1);

            //Grab outputs
            heightBuffer.GetData(heightOutputs);

            //Get rid of buffer data
            heightBuffer.Dispose();
            mapOctaveOffsetsBuffer.Dispose();
            mapRegionGPUsBuffer.Dispose();

            return heightOutputs;
        }
    }
}
