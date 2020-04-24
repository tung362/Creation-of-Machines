using System.Collections;
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
        public Run WGEditorRunInstance;

        [Header("Generator Settings")]
        public ComputeShader WGShader;
        public ComputeShader WGNoiseShader;
        public string MapSeed = "Seed";
        public Vector2Int MapSize = new Vector2Int(152, 152);
        public int ViewSize = 1;
        public float IsoSurface = 0.62f;
        [Range(1, 100)]
        public int CubesPerAxis = 30;
        public float ChunkSize = 10;
        public Vector3 MapOffset = Vector3.zero;
        public Material TestMat;
        private List<MeshFilter> TestChunks = new List<MeshFilter>();

        [Header("Surface Settings")]
        public int SurfaceOctaves = 4;
        public float SurfaceLacunarity = 1.87f;
        public float SurfaceScale = 30;

        [Header("Cave Settings")]
        public int CaveOctaves = 4;
        public float CaveLacunarity = 1.87f;
        public float CaveScale = 30;


        [Header("Voronoi Settings")]
        [Range(1, 100)]
        public int VoronoiSiteCount = 100;
        public int VoronoiRelaxationCount = 5;
        public VoronoiGradientMaskType VoronoiMaskType = VoronoiGradientMaskType.Circle;
        [Range(0.01f, 1)]
        public float VoronoiMaskRadius = 0.75f;

        /*Cache*/
        //Generation map
        public List<Region> Regions { get; private set; }
        public RegionGPU[] RegionGPUs { get; private set; }
        public Vector3[] SurfaceOctaveOffsets { get; private set; }
        public Vector3[] CaveOctaveOffsets { get; private set; }

        //Fragment shader map
        public RegionFragGPU[] RegionFragGPUs { get; private set; }
        //private float[] SurfaceHeight;
        //private float[] SurfaceFloor;
        //private float[] SurfaceAdditiveHeightLimit;

        //Builder
        //Chunk origin and parent gameobject for generated objects
        private GameObject ChunkOrigin;
        private Dictionary<Vector3Int, GameObject> Chunks = new Dictionary<Vector3Int, GameObject>(9);

        /*Minimap*/
        public GameObject RegionBorderPrefab;
        private GameObject MiniMapOrigin;

        private bool IsReady = false;

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"WorldGenerator\"");
        }

        void Start()
        {
            //Init
            Regions = new List<Region>();

            //Creates chunk origin gameobject
            ChunkOrigin = new GameObject("Chunks");
            ChunkOrigin.transform.position = Vector3.zero;

            //Creates minimap origin gameobject
            MiniMapOrigin = new GameObject("Minimap");
            MiniMapOrigin.transform.position = Vector3.zero;

            //Generates noise map data for calculations
            GenerateNoiseMap();

            //Generates markers
            GenerateRegionBorders();

            //Apply generation data to terrain material
            ApplyDataToTerrainMaterial(TestMat);

            //CreateChunk(0, 0, 0);
            //for (int x = -ViewSize; x <= ViewSize; x++)
            //{
            //    for (int z = -ViewSize; z <= ViewSize; z++)
            //    {
            //        for (int y = -ViewSize; y <= ViewSize; y++)
            //        {
            //            CreateChunk(x, y, z);
            //        }
            //    }
            //}

            for (int x = 0; x < ViewSize * 2; x++)
            {
                for (int z = 0; z < ViewSize * 2; z++)
                {
                    for (int y = -ViewSize; y < ViewSize; y++)
                    {
                        CreateChunk(x, y, z);
                    }
                }
            }
            IsReady = true;
        }

        void Update()
        {
            //if(Input.GetKeyDown(KeyCode.N))
            //{
            //    CreateChunk(0, -1, 0);
            //}
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

                //for (int x = -ViewSize; x <= ViewSize; x++)
                //{
                //    for (int z = -ViewSize; z <= ViewSize; z++)
                //    {
                //        for (int y = -ViewSize; y <= ViewSize; y++)
                //        {
                //            CreateChunk(x, y, z);
                //        }
                //    }
                //}

                for (int x = 0; x < ViewSize * 2; x++)
                {
                    for (int z = 0; z < ViewSize * 2; z++)
                    {
                        for (int y = -ViewSize; y < ViewSize; y++)
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
            GenerateVoronoiGraph(MapSize, Regions);

            //Normal map
            //MapOctaveOffsets = GenerateMapOctaveOffsets();
            RandomizeMapRegions(Regions);

            //Texture shader
            RegionFragGPUs = CreateRegionFragGPUs(Regions);

            //GPU
            RegionGPUs = CreateRegionGPUs(Regions);
        }

        void CreateChunk(int ChunkCoordX, int ChunkCoordY, int ChunkCoordZ)
        {
            (Vector3[], int[], RegionIndexFragGPU) meshData = ShaderGenerateChunk(ChunkCoordX, ChunkCoordY, ChunkCoordZ);

            GameObject chunk = new GameObject();
            chunk.transform.parent = ChunkOrigin.transform;
            chunk.transform.localPosition = new Vector3(ChunkCoordX, ChunkCoordY, ChunkCoordZ) * ChunkSize;
            MeshFilter chunkMF = chunk.AddComponent<MeshFilter>();
            MeshRenderer chunkMR = chunk.AddComponent<MeshRenderer>();
            MeshCollider chunkMC = chunk.AddComponent<MeshCollider>();

            chunkMR.material = TestMat;

            chunkMR.material.SetInt("_BiomeIndex0", meshData.Item3.Index0);
            chunkMR.material.SetInt("_BiomeIndex1", meshData.Item3.Index1);
            chunkMR.material.SetInt("_BiomeIndex2", meshData.Item3.Index2);
            chunkMR.material.SetInt("_BiomeIndex3", meshData.Item3.Index3);

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
        public (Vector3[], int[], RegionIndexFragGPU) ShaderGenerateChunk(int chunkCoordX, int chunkCoordY, int chunkCoordZ)
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
            RegionIndexFragGPU regionIndexFragGPU;
            ComputeBuffer noisePointsBuffer = ShaderGenerateNoiseChunk(chunkCoordX, chunkCoordY, chunkCoordZ, out regionIndexFragGPU);
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

            return (verticesOutput, trianglesOutput, regionIndexFragGPU);
        }

        public ComputeBuffer ShaderGenerateNoiseChunk(int chunkCoordX, int chunkCoordY, int chunkCoordZ, out RegionIndexFragGPU regionIndexFragGPUOutput)
        {
            SurfaceOctaveOffsets = GenerateMapOctaveOffsets(false);
            CaveOctaveOffsets = GenerateMapOctaveOffsets(true);

            //Total number of cubes inside a chunk
            int cubesPerChunk = CubesPerAxis * CubesPerAxis * CubesPerAxis;
            int indexesPerChunk = CubesPerAxis * CubesPerAxis;

            //How many to process on a single thread
            int processPerThread = Mathf.CeilToInt(CubesPerAxis / 8.0f);

            //Find bytes
            int sizeOfInt;
            int sizeOfVector4;
            int sizeOfVector3;
            int sizeOfMapRegionGPUs;
            unsafe
            {
                sizeOfInt = sizeof(int);
                sizeOfVector4 = sizeof(Vector4);
                sizeOfVector3 = sizeof(Vector3);
                sizeOfMapRegionGPUs = sizeof(RegionGPU);
            }

            //Kernal
            int kernel = WGNoiseShader.FindKernel("Noise");

            //RWStructuredBuffer sets
            ComputeBuffer noisePointsBuffer = new ComputeBuffer(cubesPerChunk, sizeOfVector4);
            ComputeBuffer regionIndexGPUsBuffer = new ComputeBuffer(indexesPerChunk, sizeOfInt);
            WGNoiseShader.SetBuffer(kernel, "NoisePoints", noisePointsBuffer);
            WGNoiseShader.SetBuffer(kernel, "RegionIndexes", regionIndexGPUsBuffer);

            //Sets
            WGNoiseShader.SetInts("ThreadDimensions", new int[3] { CubesPerAxis, CubesPerAxis, CubesPerAxis });
            WGNoiseShader.SetInts("ChunkCoord", new int[3] { chunkCoordX, chunkCoordY, chunkCoordZ });
            WGNoiseShader.SetFloat("ChunkSize", ChunkSize);
            WGNoiseShader.SetFloat("IsoSurface", IsoSurface);
            WGNoiseShader.SetFloat("CubesPerAxis", CubesPerAxis);
            WGNoiseShader.SetFloat("SurfaceLacunarity", SurfaceLacunarity);
            WGNoiseShader.SetFloat("SurfaceScale", SurfaceScale);
            WGNoiseShader.SetInt("SurfaceOctaves", SurfaceOctaves);
            WGNoiseShader.SetInt("Sites", RegionGPUs.Length);
            WGNoiseShader.SetFloat("CaveLacunarity", CaveLacunarity);
            WGNoiseShader.SetFloat("CaveScale", CaveScale);
            WGNoiseShader.SetInt("CaveOctaves", CaveOctaves);

            //StructuredBuffer sets
            ComputeBuffer surfaceOctaveOffsetsBuffer = new ComputeBuffer(SurfaceOctaves, sizeOfVector3);
            ComputeBuffer CaveOctaveOffsetsBuffer = new ComputeBuffer(CaveOctaves, sizeOfVector3);
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(RegionGPUs.Length, sizeOfMapRegionGPUs);
            surfaceOctaveOffsetsBuffer.SetData(SurfaceOctaveOffsets);
            CaveOctaveOffsetsBuffer.SetData(CaveOctaveOffsets);
            mapRegionGPUsBuffer.SetData(RegionGPUs);
            WGNoiseShader.SetBuffer(kernel, "SurfaceOctaveOffsets", surfaceOctaveOffsetsBuffer);
            WGNoiseShader.SetBuffer(kernel, "CaveOctaveOffsets", CaveOctaveOffsetsBuffer);
            WGNoiseShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);

            //Run kernal
            WGNoiseShader.Dispatch(kernel, processPerThread, processPerThread, processPerThread);

            //Output
            int[] regionIndexGPUOutput = new int[indexesPerChunk];
            regionIndexGPUsBuffer.GetData(regionIndexGPUOutput);

            //Process indexes for fragment shader
            RegionIndexFragGPU regionIndexFragGPU = new RegionIndexFragGPU
            {
                Index0 = -1,
                Index1 = -1,
                Index2 = -1,
                Index3 = -1,
            };
            for (int i = 0; i < regionIndexGPUOutput.Length; i++)
            {
                if (regionIndexFragGPU.Index0 == regionIndexGPUOutput[i]) continue;
                if (regionIndexFragGPU.Index0 == -1)
                {
                    regionIndexFragGPU.Index0 = regionIndexGPUOutput[i];
                    continue;
                }

                if (regionIndexFragGPU.Index1 == regionIndexGPUOutput[i]) continue;
                if (regionIndexFragGPU.Index1 == -1)
                {
                    regionIndexFragGPU.Index1 = regionIndexGPUOutput[i];
                    continue;
                }

                if (regionIndexFragGPU.Index2 == regionIndexGPUOutput[i]) continue;
                if (regionIndexFragGPU.Index2 == -1)
                {
                    regionIndexFragGPU.Index2 = regionIndexGPUOutput[i];
                    continue;
                }

                if (regionIndexFragGPU.Index3 == regionIndexGPUOutput[i]) continue;
                if (regionIndexFragGPU.Index3 == -1)
                {
                    regionIndexFragGPU.Index3 = regionIndexGPUOutput[i];
                    break;
                }
            }
            regionIndexFragGPUOutput = regionIndexFragGPU;

            //Get rid of buffer data
            surfaceOctaveOffsetsBuffer.Dispose();
            mapRegionGPUsBuffer.Dispose();

            return noisePointsBuffer;
        }
        #endregion

        #region Setup
        public void GenerateVoronoiGraph(Vector2Int mapSize, List<Region> outputMapRegions = null, List<List<Vector2>> outputRegions = null, List<LineSegment> outputSpanningTrees = null)
        {
            Rect bounds = new Rect(0, 0, mapSize.x, mapSize.y);
            List<Vector2> points = CreateRandomPoints(mapSize);

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
                    Region mapRegion = new Region
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

        public List<Vector2> CreateRandomPoints(Vector2Int mapSize)
        {
            Random.InitState(MapSeed.GetHashCode());
            List<Vector2> points = new List<Vector2>();

            Vector2 center = new Vector3(mapSize.x * 0.5f, mapSize.y * 0.5f);
            for (int i = 0; i < VoronoiSiteCount; i++)
            {
                if (VoronoiMaskType == VoronoiGradientMaskType.Circle)
                {
                    Vector2 randomPoint = UnityEngine.Random.insideUnitCircle;
                    points.Add(center + new Vector2(randomPoint.x, randomPoint.y) * ((mapSize.x * 0.5f) * VoronoiMaskRadius));
                }
                else points.Add(new Vector2(Random.Range(0, mapSize.x), Random.Range(0, mapSize.y)));
            }
            return points;
        }

        public Vector3[] GenerateMapOctaveOffsets(bool IsSubtractive)
        {
            Random.InitState(IsSubtractive ? -MapSeed.GetHashCode() : MapSeed.GetHashCode());
            Vector3[] mapOctaveOffsets = new Vector3[IsSubtractive ? CaveOctaves : SurfaceOctaves];
            for (int i = 0; i < (IsSubtractive ? CaveOctaves : SurfaceOctaves); i++)
            {
                float offsetX = Random.Range(-100000.0f, 100000.0f) + MapOffset.x;
                float offsetY = Random.Range(-100000.0f, 100000.0f) + MapOffset.y;
                float offsetZ = Random.Range(-100000.0f, 100000.0f) + MapOffset.z;
                mapOctaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
            }
            return mapOctaveOffsets;
        }

        public void RandomizeMapRegions(List<Region> mapRegions)
        {
            Random.InitState(MapSeed.GetHashCode());

            //Might change this later so that it chooses from a list of presets randomly instead of fully random
            for (int i = 0; i < mapRegions.Count; i++)
            {
                //Generate names
                //Randomizes surface and cave biomes for each region
                int surfaceBiomeRoll = Random.Range(0, !Application.isPlaying ? WGEditorRunInstance.SurfaceBiomes.Count : Run.instance.SurfaceBiomes.Count);
                int caveBiomeRoll = Random.Range(0, !Application.isPlaying ? WGEditorRunInstance.CaveBiomes.Count : Run.instance.CaveBiomes.Count);

                mapRegions[i].SurfaceBiome = !Application.isPlaying ? WGEditorRunInstance.SurfaceBiomes[surfaceBiomeRoll] : Run.instance.SurfaceBiomes[surfaceBiomeRoll];
                mapRegions[i].CaveBiome = !Application.isPlaying ? WGEditorRunInstance.CaveBiomes[caveBiomeRoll] : Run.instance.CaveBiomes[caveBiomeRoll];
            }
        }

        public RegionGPU[] CreateRegionGPUs(List<Region> mapRegions)
        {
            RegionGPU[] mapRegionGPUs = new RegionGPU[mapRegions.Count];
            for (int i = 0; i < mapRegions.Count; i++)
            {
                SurfaceBiomeGPU surfaceBiomeGPU = new SurfaceBiomeGPU
                {
                    Id = mapRegions[i].SurfaceBiome.Id,
                    Persistance = mapRegions[i].SurfaceBiome.Persistance,
                    SurfaceHeight = mapRegions[i].SurfaceBiome.SurfaceHeight,
                    SurfaceFloor = mapRegions[i].SurfaceBiome.SurfaceFloor,
                    SurfaceAdditiveHeight = mapRegions[i].SurfaceBiome.SurfaceAdditiveHeight,
                    SurfaceAdditiveHeightLimit = mapRegions[i].SurfaceBiome.SurfaceAdditiveHeightLimit,
                    SurfaceAdditiveOffset = mapRegions[i].SurfaceBiome.SurfaceAdditiveOffset
                };

                CaveBiomeGPU caveBiomeGPU = new CaveBiomeGPU
                {
                    Id = mapRegions[i].CaveBiome.Id,
                    Persistance = mapRegions[i].CaveBiome.Persistance,
                    CaveThreshold = mapRegions[i].CaveBiome.CaveThreshold,
                };

                RegionGPU regionGPU = new RegionGPU
                {
                    Coord = mapRegions[i].RegionSite.Coord,
                    SurfaceBiome = surfaceBiomeGPU,
                    CaveBiome = caveBiomeGPU
                };
                mapRegionGPUs[i] = regionGPU;
            }
            return mapRegionGPUs;
        }

        public RegionFragGPU[] CreateRegionFragGPUs(List<Region> mapRegions)
        {
            RegionFragGPU[] regionFragGPUs = new RegionFragGPU[mapRegions.Count];
            //SurfaceHeight = new float[mapRegions.Count];
            //SurfaceFloor = new float[mapRegions.Count];
            //SurfaceAdditiveHeightLimit = new float[mapRegions.Count];
            for (int i = 0; i < mapRegions.Count; i++)
            {
                //Map out closest neighbors
                List<Site> neighborSites = mapRegions[i].RegionSite.NeighborSites();
                if (neighborSites.Count > 8) Debug.Log("Warning! Neighbor site count greater than 8! Count: " + neighborSites.Count + " @CreateRegionFragGPUs(List<Region> mapRegions)");

                //SurfaceHeight[i] = mapRegions[i].SurfaceBiome.SurfaceHeight;
                //SurfaceFloor[i] = mapRegions[i].SurfaceBiome.SurfaceFloor;
                //SurfaceAdditiveHeightLimit[i] = mapRegions[i].SurfaceBiome.SurfaceAdditiveHeightLimit;

                RegionFragGPU regionFragGPU = new RegionFragGPU
                {
                    Coord = mapRegions[i].RegionSite.Coord,
                    SurfaceBiomeID = mapRegions[i].SurfaceBiome.Id,
                    CaveBiomeID = mapRegions[i].CaveBiome.Id,
                    //SurfaceHeight = mapRegions[i].SurfaceBiome.SurfaceHeight,
                    //SurfaceFloor = mapRegions[i].SurfaceBiome.SurfaceFloor,
                    //SurfaceAdditiveHeightLimit = mapRegions[i].SurfaceBiome.SurfaceAdditiveHeightLimit,
                    SurfaceGroundGradient = CreateGradientFragGPU(mapRegions[i].SurfaceBiome.GroundPalette),
                    SurfaceWallGradient = CreateGradientFragGPU(mapRegions[i].SurfaceBiome.WallPalette),
                    CaveGroundGradient = CreateGradientFragGPU(mapRegions[i].CaveBiome.GroundPalette),
                    CaveWallGradient = CreateGradientFragGPU(mapRegions[i].CaveBiome.WallPalette),

                    NeighborIndex0 = 0 < neighborSites.Count ? neighborSites[0].SiteIndex : -1,
                    NeighborIndex1 = 1 < neighborSites.Count ? neighborSites[1].SiteIndex : -1,
                    NeighborIndex2 = 2 < neighborSites.Count ? neighborSites[2].SiteIndex : -1,
                    NeighborIndex3 = 3 < neighborSites.Count ? neighborSites[3].SiteIndex : -1,
                    NeighborIndex4 = 4 < neighborSites.Count ? neighborSites[4].SiteIndex : -1,
                    NeighborIndex5 = 5 < neighborSites.Count ? neighborSites[5].SiteIndex : -1,
                    NeighborIndex6 = 6 < neighborSites.Count ? neighborSites[6].SiteIndex : -1,
                    NeighborIndex7 = 7 < neighborSites.Count ? neighborSites[7].SiteIndex : -1
                };
                regionFragGPUs[i] = regionFragGPU;
            }
            return regionFragGPUs;
        }

        public GradientFragGPU CreateGradientFragGPU(Gradient gradient)
        {
            GradientFragGPU gradientFragGPU = new GradientFragGPU
            {
                ColorLength = gradient.colorKeys.Length,
                C0 = 0 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[0].color.r, gradient.colorKeys[0].color.g, gradient.colorKeys[0].color.b, gradient.colorKeys[0].time) : Vector4.zero,
                C1 = 1 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[1].color.r, gradient.colorKeys[1].color.g, gradient.colorKeys[1].color.b, gradient.colorKeys[1].time) : Vector4.zero,
                C2 = 2 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[2].color.r, gradient.colorKeys[2].color.g, gradient.colorKeys[2].color.b, gradient.colorKeys[2].time) : Vector4.zero,
                C3 = 3 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[3].color.r, gradient.colorKeys[3].color.g, gradient.colorKeys[3].color.b, gradient.colorKeys[3].time) : Vector4.zero,
                C4 = 4 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[4].color.r, gradient.colorKeys[4].color.g, gradient.colorKeys[4].color.b, gradient.colorKeys[4].time) : Vector4.zero,
                C5 = 5 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[5].color.r, gradient.colorKeys[5].color.g, gradient.colorKeys[5].color.b, gradient.colorKeys[5].time) : Vector4.zero,
                C6 = 6 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[6].color.r, gradient.colorKeys[6].color.g, gradient.colorKeys[6].color.b, gradient.colorKeys[6].time) : Vector4.zero,
                C7 = 7 < gradient.colorKeys.Length ? new Vector4(gradient.colorKeys[7].color.r, gradient.colorKeys[7].color.g, gradient.colorKeys[7].color.b, gradient.colorKeys[7].time) : Vector4.zero,

            };
            return gradientFragGPU;
        }

        public Texture2D CreateTestTexture(List<Region> mapRegions)
        {
            Texture2D test = new Texture2D(mapRegions.Count * 3, 1, TextureFormat.RFloat, false, true);

            for (int i = 0; i < mapRegions.Count * 3; i += 3)
            {
                test.SetPixel(i, 0, GameTools.EncodeFloatRGBA(mapRegions[i / 3].SurfaceBiome.SurfaceHeight / 300));
                test.SetPixel(i + 1, 0, GameTools.EncodeFloatRGBA(mapRegions[i / 3].SurfaceBiome.SurfaceFloor / 300));
                test.SetPixel(i + 2, 0, GameTools.EncodeFloatRGBA(mapRegions[i / 3].SurfaceBiome.SurfaceAdditiveHeightLimit / 300));
            }

            test.wrapMode = TextureWrapMode.Clamp;
            test.filterMode = FilterMode.Point;
            test.Apply();

            //for (int i = 0; i < mapRegions.Count * 3; i++)
            //{
            //    Debug.Log();
            //}

            //((float)regionIndex * 3) / 300
            //float res = GameTools.DecodeFloatRGBA(test.GetPixel((0 * 3) + 0, 0)) * 300;
            //for (int i = 0; i < mapRegions.Count; i++)
            //{
            //    Debug.Log(GameTools.DecodeFloatRGBA(test.GetPixel((i * 3) + 2, 0)) * 300);
            //}
            //Debug.Log(res);

            return test;
        }

        public void ApplyDataToTerrainMaterial(Material mat)
        {
            //Find bytes
            int sizeOfRegionFragGPU;
            unsafe
            {
                sizeOfRegionFragGPU = sizeof(RegionFragGPU);
            }

            //RWStructuredBuffer sets
            ComputeBuffer regionFragGPUBuffer = new ComputeBuffer(RegionFragGPUs.Length, sizeOfRegionFragGPU);
            regionFragGPUBuffer.SetData(RegionFragGPUs);
            mat.SetBuffer("RegionFragGPUs", regionFragGPUBuffer);

            //mat.SetFloatArray("SurfaceHeight", SurfaceHeight);
            //mat.SetFloatArray("SurfaceFloor", SurfaceFloor);
            //mat.SetFloatArray("SurfaceAdditiveHeightLimit", SurfaceAdditiveHeightLimit);

            mat.SetTexture("_Test", CreateTestTexture(Regions));
        }
        #endregion

        #region Marker
        void GenerateRegionBorders()
        {
            //Create edge borders
            GameObject outOfBoundsBorder = GameObject.Instantiate(RegionBorderPrefab, MiniMapOrigin.transform);
            LineRenderer outOfBoundsLR = outOfBoundsBorder.GetComponent<LineRenderer>();
            Vector3[] outOfBoundsEdges = new Vector3[5];
            outOfBoundsLR.positionCount = 5;
            outOfBoundsEdges[0] = new Vector3(0, 5, 0);
            outOfBoundsEdges[1] = new Vector3(0, 5, MapSize.y);
            outOfBoundsEdges[2] = new Vector3(MapSize.x, 5, MapSize.y);
            outOfBoundsEdges[3] = new Vector3(MapSize.x, 5, 0);
            outOfBoundsEdges[4] = new Vector3(0, 5, 0);
            outOfBoundsLR.SetPositions(outOfBoundsEdges);

            //Create region borders
            for (int i = 0; i < Regions.Count; i++)
            {
                for (int j = 0; j < Regions[i].RegionSite.Edges.Count; j++)
                {
                    if (Regions[i].RegionSite.Edges[j].ClippedEnds == null) continue;

                    GameObject regionBorder = GameObject.Instantiate(RegionBorderPrefab, MiniMapOrigin.transform);
                    LineRenderer lineRenderer = regionBorder.GetComponent<LineRenderer>();
                    Vector3[] borderEdges = new Vector3[2];
                    lineRenderer.positionCount = 2;

                    Vector3 left = new Vector3(Regions[i].RegionSite.Edges[j].ClippedEnds[LR.LEFT].x, 5, Regions[i].RegionSite.Edges[j].ClippedEnds[LR.LEFT].y);
                    Vector3 right = new Vector3(Regions[i].RegionSite.Edges[j].ClippedEnds[LR.RIGHT].x, 5, Regions[i].RegionSite.Edges[j].ClippedEnds[LR.RIGHT].y);

                    borderEdges[0] = left;
                    borderEdges[1] = right;

                    lineRenderer.SetPositions(borderEdges);
                }
            }
        }
        #endregion
    }
}
