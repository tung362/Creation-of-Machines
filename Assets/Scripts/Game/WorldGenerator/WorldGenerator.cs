﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

namespace COM
{
    public class WorldGenerator : MonoBehaviour
    {
        public static WorldGenerator instance { get; private set; }

        [Header("Editor Settings")]
        public ComputeShader WGEditorShader;

        [Header("Generator Settings")]
        public ComputeShader WGShader;
        public ComputeShader WGNoiseShader;
        public string MapSeed = "Seed";
        public Vector2Int MapSize = new Vector2Int(152, 152);
        public int ViewSize = 1;
        public float IsoSurface = 0.62f;
        public int CubesPerAxis = 30;
        public float ChunkSize = 10;
        public Vector3 MapOffset = Vector3.zero;
        public GameObject TestSpawn;
        public Material TestMat;
        private List<MeshFilter> TestChunks = new List<MeshFilter>();

        [Header("Surface Settings")]
        public int SurfaceOctaves = 4;
        public float SurfaceScale = 30;
        public float SurfacePersistance = 0.53f;
        public float SurfaceLacunarity = 1.87f;
        public float SurfaceFloor = 12;
        public float SurfaceHeight = 10;

        [Header("Cave Settings")]
        public float CaveThreshold = 0.62f;
        public int CaveOctaves = 4;
        public float CaveScale = 30;
        public float CavePersistance = 0.53f;
        public float CaveLacunarity = 1.87f;


        [Header("Voronoi Settings")]
        public int VoronoiSiteCount = 100;
        public int VoronoiRelaxationCount = 5;
        public VoronoiGradientMaskType VoronoiMaskType = VoronoiGradientMaskType.Circle;
        [Range(0.01f, 1)]
        public float VoronoiMaskRadius = 0.75f;

        /*Cache*/
        //Generation map
        public List<MapRegion> MapRegions { get; private set; }
        public MapRegionGPU[] MapRegionGPUs { get; private set; }
        public Vector3[] SurfaceOctaveOffsets { get; private set; }
        public Vector3[] CaveOctaveOffsets { get; private set; }

        //Builder
        //Chunk origin and parent gameobject for generated objects
        private GameObject ChunkOrigin;
        private Dictionary<Vector3Int, GameObject> Chunks = new Dictionary<Vector3Int, GameObject>(9);

        private bool IsReady = false;

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"WorldGenerator\"");
        }

        void Awake()
        {
            //Init
            MapRegions = new List<MapRegion>();

            //Creates chunk origin gameobject
            ChunkOrigin = new GameObject("Chunks");
            ChunkOrigin.transform.position = Vector3.zero;

            //Generates noise map data for calculations
            GenerateNoiseMap();

            //CreateChunk(0, 0, 0);
            for (int x = -ViewSize; x <= ViewSize; x++)
            {
                for (int z = -ViewSize; z <= ViewSize; z++)
                {
                    for (int y = -ViewSize; y <= ViewSize; y++)
                    {
                        CreateChunk(x, y, z);
                    }
                }
            }
            IsReady = true;
        }

        void Update()
        {
        }

        void OnValidate()
        {
            if(Application.isPlaying && IsReady)
            {
                for (int i = 0; i < TestChunks.Count; i++)
                {
                    Destroy(TestChunks[i].sharedMesh);
                    Destroy(TestChunks[i].gameObject);
                    TestChunks.RemoveAt(i);
                    i--;
                }

                for (int x = -ViewSize; x <= ViewSize; x++)
                {
                    for (int z = -ViewSize; z <= ViewSize; z++)
                    {
                        for (int y = -ViewSize; y <= ViewSize; y++)
                        {
                            CreateChunk(x, y, z);
                        }
                    }
                }
            }
        }

        #region Generation
        void GenerateNoiseMap()
        {
            //Voronoi map
            GenerateVoronoiGraph(MapRegions);

            //Normal map
            //MapOctaveOffsets = GenerateMapOctaveOffsets();
            RandomizeMapRegions(MapRegions, false);
            RandomizeMapRegions(MapRegions, true);

            //GPU
            MapRegionGPUs = CreateMapRegionGPUs(MapRegions);
        }

        void CreateChunk(int ChunkCoordX, int ChunkCoordY, int ChunkCoordZ)
        {
            (Vector3[], int[]) meshData = ShaderGenerateChunk(ChunkCoordX, ChunkCoordY, ChunkCoordZ);

            GameObject chunk = new GameObject();
            chunk.transform.parent = ChunkOrigin.transform;
            chunk.transform.localPosition = new Vector3(ChunkCoordX, ChunkCoordY, ChunkCoordZ) * ChunkSize;
            MeshFilter chunkMF = chunk.AddComponent<MeshFilter>();
            MeshRenderer chunkMR = chunk.AddComponent<MeshRenderer>();

            chunkMR.material = TestMat;

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = meshData.Item1;
            mesh.triangles = meshData.Item2;
            mesh.RecalculateNormals();
            chunkMF.sharedMesh = mesh;
            TestChunks.Add(chunkMF);
        }
        #endregion

        #region ComputeShader
        public (Vector3[], int[]) ShaderGenerateChunk(int chunkCoordX, int chunkCoordY, int chunkCoordZ)
        {
            //Max TriangleGPU count inside a chunk, 5 possible triangles inside of TriTable, 3 pairs per triangle
            int maxTriangleGPUCount = (CubesPerAxis * CubesPerAxis * CubesPerAxis) * 5;

            //How many to process on a single thread
            int processPerThread = Mathf.CeilToInt(CubesPerAxis / 8.0f);

            //Find bytes
            int sizeOfInt;
            int sizeOfTriangleGPU;
            unsafe
            {
                sizeOfInt = sizeof(int);
                sizeOfTriangleGPU = sizeof(TriangleGPU);
            }

            //Kernal
            int kernel = WGShader.FindKernel("GenerateChunk");

            //RWStructuredBuffer sets
            ComputeBuffer trianglesBuffer = new ComputeBuffer(maxTriangleGPUCount, sizeOfTriangleGPU, ComputeBufferType.Append);
            ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeOfInt, ComputeBufferType.Raw);
            trianglesBuffer.SetCounterValue(0);
            WGShader.SetBuffer(kernel, "Triangles", trianglesBuffer);

            //Sets
            WGShader.SetInts("ThreadDimensions", new int[3] { CubesPerAxis, CubesPerAxis, CubesPerAxis });
            WGShader.SetFloat("IsoSurface", IsoSurface);
            WGShader.SetFloat("CubesPerAxis", CubesPerAxis);

            //Run noise kernal
            ComputeBuffer noisePointsBuffer = ShaderGenerateNoiseChunk(chunkCoordX, chunkCoordY, chunkCoordZ);
            WGShader.SetBuffer(kernel, "NoisePoints", noisePointsBuffer);

            //Run kernal
            WGShader.Dispatch(kernel, processPerThread, processPerThread, processPerThread);

            //Grab output count
            ComputeBuffer.CopyCount(trianglesBuffer, triCountBuffer, 0);
            int[] triCount = { 0 };
            triCountBuffer.GetData(triCount);

            //Outputs
            TriangleGPU[] triangleGPUOutput = new TriangleGPU[triCount[0]];
            Vector3[] verticesOutput = new Vector3[triangleGPUOutput.Length * 3];
            int[] trianglesOutput = new int[triangleGPUOutput.Length * 3];

            //Grab outputs
            trianglesBuffer.GetData(triangleGPUOutput, 0, 0, triCount[0]);

            //Get rid of buffer data
            trianglesBuffer.Dispose();
            triCountBuffer.Dispose();
            noisePointsBuffer.Dispose();

            //Process triangles
            for (int i = 0; i < triangleGPUOutput.Length; i++)
            {
                verticesOutput[i * 3] = triangleGPUOutput[i].vertexA;
                verticesOutput[(i * 3) + 1] = triangleGPUOutput[i].vertexB;
                verticesOutput[(i * 3) + 2] = triangleGPUOutput[i].vertexC;

                trianglesOutput[i * 3] = i * 3;
                trianglesOutput[(i * 3) + 1] = (i * 3) + 1;
                trianglesOutput[(i * 3) + 2] = (i * 3) + 2;
            }

            return (verticesOutput, trianglesOutput);
        }

        public ComputeBuffer ShaderGenerateNoiseChunk(int chunkCoordX, int chunkCoordY, int chunkCoordZ)
        {
            SurfaceOctaveOffsets = GenerateMapOctaveOffsets(false);
            CaveOctaveOffsets = GenerateMapOctaveOffsets(true);

            //Total number of cubes inside a chunk
            int cubesPerChunk = CubesPerAxis * CubesPerAxis * CubesPerAxis;

            //How many to process on a single thread
            int processPerThread = Mathf.CeilToInt(CubesPerAxis / 8.0f);

            //Find bytes
            int sizeOfVector4;
            int sizeOfVector3;
            int sizeOfMapRegionGPUs;
            unsafe
            {
                sizeOfVector4 = sizeof(Vector4);
                sizeOfVector3 = sizeof(Vector3);
                sizeOfMapRegionGPUs = sizeof(MapRegionGPU);
            }

            //Kernal
            int kernel = WGNoiseShader.FindKernel("Noise");

            //RWStructuredBuffer sets
            ComputeBuffer noisePointsBuffer = new ComputeBuffer(cubesPerChunk, sizeOfVector4);
            WGNoiseShader.SetBuffer(kernel, "NoisePoints", noisePointsBuffer);

            //Sets
            WGNoiseShader.SetInts("ThreadDimensions", new int[3] { CubesPerAxis, CubesPerAxis, CubesPerAxis });
            WGNoiseShader.SetInts("ChunkCoord", new int[3] { chunkCoordX, chunkCoordY, chunkCoordZ });
            WGNoiseShader.SetFloat("ChunkSize", ChunkSize);
            WGNoiseShader.SetFloat("IsoSurface", IsoSurface);
            WGNoiseShader.SetFloat("CaveThreshold", CaveThreshold);
            WGNoiseShader.SetFloat("CubesPerAxis", CubesPerAxis);
            WGNoiseShader.SetFloat("SurfaceScale", SurfaceScale);
            WGNoiseShader.SetFloat("SurfacePersistance", SurfacePersistance);
            WGNoiseShader.SetFloat("SurfaceLacunarity", SurfaceLacunarity);
            WGNoiseShader.SetInt("SurfaceOctaves", SurfaceOctaves);
            WGNoiseShader.SetInt("Sites", MapRegionGPUs.Length);
            WGNoiseShader.SetFloat("TerrainHeight", SurfaceHeight);
            WGNoiseShader.SetFloat("TerrainFloor", SurfaceFloor);
            WGNoiseShader.SetFloat("CaveScale", CaveScale);
            WGNoiseShader.SetFloat("CavePersistance", CavePersistance);
            WGNoiseShader.SetFloat("CaveLacunarity", CaveLacunarity);
            WGNoiseShader.SetInt("CaveOctaves", CaveOctaves);

            //StructuredBuffer sets
            ComputeBuffer surfaceOctaveOffsetsBuffer = new ComputeBuffer(SurfaceOctaves, sizeOfVector3);
            ComputeBuffer CaveOctaveOffsetsBuffer = new ComputeBuffer(CaveOctaves, sizeOfVector3);
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(MapRegionGPUs.Length, sizeOfMapRegionGPUs);
            surfaceOctaveOffsetsBuffer.SetData(SurfaceOctaveOffsets);
            CaveOctaveOffsetsBuffer.SetData(CaveOctaveOffsets);
            mapRegionGPUsBuffer.SetData(MapRegionGPUs);
            WGNoiseShader.SetBuffer(kernel, "SurfaceOctaveOffsets", surfaceOctaveOffsetsBuffer);
            WGNoiseShader.SetBuffer(kernel, "CaveOctaveOffsets", CaveOctaveOffsetsBuffer);
            WGNoiseShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);

            //Run kernal
            WGNoiseShader.Dispatch(kernel, processPerThread, processPerThread, processPerThread);

            //Get rid of buffer data
            surfaceOctaveOffsetsBuffer.Dispose();
            mapRegionGPUsBuffer.Dispose();

            return noisePointsBuffer;
        }
        #endregion

        #region Setup
        public void GenerateVoronoiGraph(List<MapRegion> outputMapRegions = null, List<List<Vector2>> outputRegions = null, List<LineSegment> outputSpanningTrees = null)
        {
            Rect bounds = new Rect(0, 0, MapSize.x, MapSize.y);
            List<Vector2> points = CreateRandomPoints();

            //Generates new graph
            Voronoi voronoi = new Voronoi(points, bounds, VoronoiRelaxationCount);

            //Results
            if (outputMapRegions != null)
            {
                outputMapRegions.Clear();
                List<Vector2> siteCoords = voronoi.SiteCoords();
                Dictionary<Vector2, Site> siteDictionary = voronoi.SitesIndexedByLocation;
                for (int i = 0; i < siteCoords.Count; i++)
                {
                    MapRegion mapRegion = new MapRegion
                    {
                        RegionSite = siteDictionary[siteCoords[i]]
                    };
                    outputMapRegions.Add(mapRegion);
                }
            }
            if (outputRegions != null)
            {
                outputRegions.Clear();
                outputRegions.AddRange(voronoi.Regions());
            }
            if (outputSpanningTrees != null)
            {
                outputSpanningTrees.Clear();
                outputSpanningTrees.AddRange(voronoi.SpanningTree(KruskalType.MINIMUM));
            }
        }

        public List<Vector2> CreateRandomPoints()
        {
            Random.InitState(MapSeed.GetHashCode());
            List<Vector2> points = new List<Vector2>();

            Vector2 center = new Vector3(MapSize.x * 0.5f, MapSize.y * 0.5f);
            for (int i = 0; i < VoronoiSiteCount; i++)
            {
                if (VoronoiMaskType == VoronoiGradientMaskType.Circle)
                {
                    Vector2 randomPoint = UnityEngine.Random.insideUnitCircle;
                    points.Add(center + new Vector2(randomPoint.x, randomPoint.y) * ((MapSize.x * 0.5f) * VoronoiMaskRadius));
                }
                else points.Add(new Vector2(Random.Range(0, MapSize.x), Random.Range(0, MapSize.y)));
            }
            return points;
        }

        public Vector3[] GenerateMapOctaveOffsets(bool IsSubtractive)
        {
            Random.InitState(IsSubtractive ? -MapSeed.GetHashCode() : MapSeed.GetHashCode());
            Vector3[] mapOctaveOffsets = new Vector3[SurfaceOctaves];
            for (int i = 0; i < SurfaceOctaves; i++)
            {
                float offsetX = Random.Range(-100000.0f, 100000.0f) + MapOffset.x;
                float offsetY = Random.Range(-100000.0f, 100000.0f) + MapOffset.y;
                float offsetZ = Random.Range(-100000.0f, 100000.0f) + MapOffset.z;
                mapOctaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
            }
            return mapOctaveOffsets;
        }

        public void RandomizeMapRegions(List<MapRegion> mapRegions, bool IsSubtractive)
        {
            Random.InitState(IsSubtractive ? -MapSeed.GetHashCode() : MapSeed.GetHashCode());

            //Might change this later so that it chooses from a list of presets randomly instead of fully random
            for (int i = 0; i < mapRegions.Count; i++)
            {
                //Generate names
                //Generate modifiers
                int chance = Random.Range(0, 2);
                if (chance == 0)
                {
                    if (!IsSubtractive)
                    {
                        mapRegions[i].RegionType = 1;
                        mapRegions[i].GenerationModifier = 2.0f;
                    }
                    else
                    {
                        mapRegions[i].CaveRegionType = 1;
                        mapRegions[i].GenerationCaveModifier = 2.0f;
                    }
                }
                else
                {
                    if (!IsSubtractive)
                    {
                        mapRegions[i].RegionType = 0;
                        mapRegions[i].GenerationModifier = 0.0f;
                    }
                    else
                    {
                        mapRegions[i].CaveRegionType = 0;
                        mapRegions[i].GenerationCaveModifier = 0.0f;
                    }
                }
            }
        }

        public MapRegionGPU[] CreateMapRegionGPUs(List<MapRegion> mapRegions)
        {
            MapRegionGPU[] mapRegionGPUs = new MapRegionGPU[mapRegions.Count];
            for (int i = 0; i < mapRegions.Count; i++)
            {
                MapRegionGPU mapRegionGPU = new MapRegionGPU
                {
                    RegionType = mapRegions[i].RegionType,
                    CaveRegionType = mapRegions[i].CaveRegionType,
                    Coord = mapRegions[i].RegionSite.Coord,
                    GenerationModifier = mapRegions[i].GenerationModifier,
                    GenerationCaveModifier = mapRegions[i].GenerationCaveModifier
                };
                mapRegionGPUs[i] = mapRegionGPU;
            }
            return mapRegionGPUs;
        }
        #endregion
    }
}
