using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

namespace COM.Test2
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
        public float IsoSurface = 10;
        public int CubesPerAxis = 30;
        public float ChunkSize = 10;
        public float TerrainHeight = 10;
        public GameObject TestSpawn;
        public Material TestMat;
        private List<MeshFilter> TestChunks = new List<MeshFilter>();

        [Header("Base Settings")]
        public float MapScale = 30;
        public int MapOctaves = 4;
        public float MapPersistance = 0.53f;
        public float MapLacunarity = 1.87f;
        public Vector2 MapOffset = Vector2.zero;
        public GenerationThreshold MapLayers;

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
        public Vector2[] MapOctaveOffsets { get; private set; }
        public Vector2[] SubMapOctaveOffsets { get; private set; }

        //Builder
        //Chunk origin and parent gameobject for generated objects
        private GameObject ChunkOrigin;
        private Dictionary<Vector3Int, GameObject> Chunks = new Dictionary<Vector3Int, GameObject>(9);

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"WorldGenerator\"");
        }

        void Start()
        {
            //Init
            MapRegions = new List<MapRegion>();

            //Creates chunk origin gameobject
            ChunkOrigin = new GameObject("Chunks");
            ChunkOrigin.transform.position = Vector3.zero;

            //Generates noise map data for calculations
            GenerateNoiseMap();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        CreateChunk(x, y, z);
                    }
                }
            }
        }

        void Update()
        {
            //for (int i = 0; i < TestChunks.Count; i++)
            //{
            //    Destroy(TestChunks[i].sharedMesh);
            //    Destroy(TestChunks[i].gameObject);
            //    TestChunks.RemoveAt(i);
            //    i--;
            //}
            //CreateChunk(0, 0, 0);
            //CreateChunk(1, 0, 0);
            //CreateChunk(-1, 0, 0);
            //CreateChunk(0, 0, 1);
            //CreateChunk(0, 0, -1);

            //CreateChunk(0, 1, 0);
            //CreateChunk(1, 1, 0);
            //CreateChunk(-1, 1, 0);
            //CreateChunk(0, 1, 1);
            //CreateChunk(0, 1, -1);
            //for (int x = -1; x <= 1; x++)
            //{
            //    for (int z = -1; z <= 1; z++)
            //    {
            //        for (int y = -1; y <= 1; y++)
            //        {
            //            CreateChunk(x, y, z);
            //        }
            //    }
            //}
        }

        #region Generation
        void GenerateNoiseMap()
        {
            //Voronoi map
            GenerateVoronoiGraph(MapRegions);

            //Normal map
            MapOctaveOffsets = GenerateMapOctaveOffsets(false);
            RandomizeMapRegions(MapRegions, false);

            //Subtractive map
            SubMapOctaveOffsets = GenerateMapOctaveOffsets(true);
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
            //Total number of cubes inside a chunk
            int cubesPerChunk = CubesPerAxis * CubesPerAxis * CubesPerAxis;

            //How many to process on a single thread
            int processPerThread = Mathf.CeilToInt(CubesPerAxis / 8.0f);

            //Find bytes
            int sizeOfVector4;
            int sizeOfVector2;
            int sizeOfMapRegionGPUs;
            unsafe
            {
                sizeOfVector4 = sizeof(Vector4);
                sizeOfVector2 = sizeof(Vector2);
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
            WGNoiseShader.SetFloat("CubesPerAxis", CubesPerAxis);
            WGNoiseShader.SetFloat("MapScale", MapScale);
            WGNoiseShader.SetFloat("MapPersistance", MapPersistance);
            WGNoiseShader.SetFloat("MapLacunarity", MapLacunarity);
            WGNoiseShader.SetInt("Octaves", MapOctaves);
            WGNoiseShader.SetInt("Sites", MapRegionGPUs.Length);
            WGNoiseShader.SetFloat("TerrainHeight", TerrainHeight);

            //StructuredBuffer sets
            ComputeBuffer mapOctaveOffsetsBuffer = new ComputeBuffer(MapOctaves, sizeOfVector2);
            ComputeBuffer mapSubOctaveOffsetsBuffer = new ComputeBuffer(MapOctaves, sizeOfVector2);
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(MapRegionGPUs.Length, sizeOfMapRegionGPUs);
            mapOctaveOffsetsBuffer.SetData(MapOctaveOffsets);
            mapSubOctaveOffsetsBuffer.SetData(SubMapOctaveOffsets);
            mapRegionGPUsBuffer.SetData(MapRegionGPUs);
            WGNoiseShader.SetBuffer(kernel, "MapOctaveOffsets", mapOctaveOffsetsBuffer);
            WGNoiseShader.SetBuffer(kernel, "SubMapOctaveOffsets", mapSubOctaveOffsetsBuffer);
            WGNoiseShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);

            //Run kernal
            WGNoiseShader.Dispatch(kernel, processPerThread, processPerThread, processPerThread);

            //Get rid of buffer data
            mapOctaveOffsetsBuffer.Dispose();
            mapSubOctaveOffsetsBuffer.Dispose();
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

        public Vector2[] GenerateMapOctaveOffsets(bool IsSubtractive)
        {
            Random.InitState(IsSubtractive ? -MapSeed.GetHashCode() : MapSeed.GetHashCode());
            Vector2[] mapOctaveOffsets = new Vector2[MapOctaves];
            for (int i = 0; i < MapOctaves; i++)
            {
                float offsetX = Random.Range(-100000.0f, 100000.0f) + MapOffset.x;
                float offsetY = Random.Range(-100000.0f, 100000.0f) + MapOffset.y;
                mapOctaveOffsets[i] = new Vector2(offsetX, offsetY);
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

        public KeyFrameGPU[] CreateMapRegionGPUs(Keyframe[] keyframes)
        {
            KeyFrameGPU[] keyFrameGPUs = new KeyFrameGPU[keyframes.Length];
            for (int i = 0; i < keyframes.Length; i++)
            {
                KeyFrameGPU keyFrameGPU = new KeyFrameGPU
                {
                    FrameTime = keyframes[i].time,
                    FrameValue = keyframes[i].value
                };
                keyFrameGPUs[i] = keyFrameGPU;
            }
            return keyFrameGPUs;
        }
        #endregion
    }
}
