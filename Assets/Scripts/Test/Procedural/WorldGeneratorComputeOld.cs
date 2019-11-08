using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using COM;

namespace COM.Test
{
    public class WorldGeneratorComputeOld : MonoBehaviour
    {
        public static WorldGeneratorComputeOld instance { get; private set; }

        [Header("Editor Settings")]
        public ComputeShader WGEditorShader;

        [Header("Generator Settings")]
        public ComputeShader WGShader;
        public ComputeShader ChunkShader;
        public string MapSeed = "Seed";
        public Vector2Int MapSize = new Vector2Int(100, 100);
        public int MapChunkSize = 16;
        public float MapScale = 20;
        public int MapOctaves = 4;
        public float MapPersistance = 0.5f;
        public float MapLacunarity = 1.87f;
        public Vector2 MapOffset = Vector2.zero;
        public GenerationThreshold MapLayers;

        [Header("Voronoi Settings")]
        public int VoronoiSiteCount = 100;
        public int VoronoiRelaxationCount = 5;
        public VoronoiGradientMaskType VoronoiMaskType = VoronoiGradientMaskType.None;
        [Range(0.01f, 1)]
        public float VoronoiMaskRadius = 1.0f;

        [Header("Builder Settings")]
        //Stores all unique patterns
        public List<GridPattern> GridPatternTemplates = new List<GridPattern>();
        public float SnapOffset = 0.68f;
        public GameObject GridTilePrefab;
        public GameObject TestObject;

        /*Generation Data*/
        public List<MapRegion> MapRegions { get; private set; }
        public MapRegionGPU[] MapRegionGPUs { get; private set; }
        public KeyFrameGPU[] KeyFrameGPUs { get; private set; }
        public KDTree VoronoiKDTree { get; private set; }
        public Vector2[] MapOctaveOffsets { get; private set; }
        public Vector2[] SubMapOctaveOffsets { get; private set; }
        public float MapChunkOffset { get; private set; }
        public Vector2 Center { get; private set; }

        /*Builder Data*/
        //Grid origin and parent object for the house
        private GameObject ChunkOrigin;

        //3x3x3 view port | 8x8x8 chunk
        //8 * 3 = 24 | 24x24x24 = 13824 preallocated grid tiles
        //24 + 1 = 25 | 25x25x25 = 15625 preallocated grid sub tiles
        Dictionary<Vector3Int, GameObject> GridTiles = new Dictionary<Vector3Int, GameObject>(13824);
        Dictionary<Vector3Int, GameObject> GridSubtiles = new Dictionary<Vector3Int, GameObject>(15625);
        //Stores all possible varients of each pattern
        private Dictionary<TileDataGPU, GridPatternVarient> GridPatterns = new Dictionary<TileDataGPU, GridPatternVarient>();

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"WorldGeneratorCompute\"");
        }

        void Start()
        {
            //Setup
            MapRegions = new List<MapRegion>();
            MapChunkOffset = SnapOffset * MapChunkSize;
            ChunkOrigin = new GameObject("Chunks");
            ChunkOrigin.transform.position = Vector3.zero;

            //Generates noise map data for calculations
            Setup();
            CreateAllPatternVarients();

            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        CreateChunk(x, y, z);
                    }
                }
            }

            //CreateChunk(0, 0, 0);
            //CreateChunk(0, 1, 0);
        }

        void Update()
        {

        }

        void OnValidate()
        {

        }

        #region Generation
        void Setup()
        {
            //Voronoi map
            GenerateVoronoiGraph(MapRegions);
            VoronoiKDTree = CreateKDTree(MapRegions);

            //Normal map
            MapOctaveOffsets = GenerateMapOctaveOffsets(false);
            RandomizeMapRegions(MapRegions, false);

            //Subtractive map
            SubMapOctaveOffsets = GenerateMapOctaveOffsets(true);
            RandomizeMapRegions(MapRegions, true);

            //GPU
            MapRegionGPUs = CreateMapRegionGPUs(MapRegions);
            KeyFrameGPUs = CreateMapRegionGPUs(MapLayers.LayerThreshold.keys);

            //Map center
            Center = new Vector2((float)MapSize.x / 2, (float)MapSize.y / 2);
        }

        void CreateChunk(int ChunkCoordX, int ChunkCoordY, int ChunkCoordZ)
        {
            (GridTileGPU[], GridSubtileGPU[]) noises = ShaderGenerateChunk(ChunkCoordX, ChunkCoordY, ChunkCoordZ, MapOctaveOffsets, SubMapOctaveOffsets, MapRegionGPUs, KeyFrameGPUs);

            float subtileOffset = (SnapOffset * 0.5f);

            for (int i = 0; i < noises.Item1.Length; i++)
            {
                if (noises.Item1[i].IsEmpty == 0)
                {
                    GameObject gridTile = Instantiate(GridTilePrefab, Vector3.zero, Quaternion.identity, ChunkOrigin.transform);
                    gridTile.transform.localPosition = new Vector3(noises.Item1[i].Coord.x * SnapOffset, noises.Item1[i].Coord.y * SnapOffset, noises.Item1[i].Coord.z * SnapOffset);
                    gridTile.transform.localRotation = Quaternion.identity;
                }
            }

            for (int i = 0; i < noises.Item2.Length; i++)
            {
                if (noises.Item2[i].TileData != new TileDataGPU(0, 0, 0, 0, 0, 0, 0, 0))
                {
                    for (int j = 0; j < GridPatterns[noises.Item2[i].TileData].TileTypes.Length; j++)
                    {
                        GameObject gridTile = Instantiate(RunOld.instance.OuterTiles[new OuterHouseTilesIndex(0, GridPatterns[noises.Item2[i].TileData].TileTypes[j])].TileVariants[0], Vector3.zero, Quaternion.identity, ChunkOrigin.transform);
                        gridTile.transform.localPosition = new Vector3((noises.Item2[i].Coord.x * SnapOffset) - subtileOffset, (noises.Item2[i].Coord.y * SnapOffset) - subtileOffset, (noises.Item2[i].Coord.z * SnapOffset) - subtileOffset);
                        gridTile.transform.localEulerAngles = new Vector3(0, j == 0 ? GridPatterns[noises.Item2[i].TileData].RotationAngle : GridPatterns[new TileDataGPU(noises.Item2[i].TileData.d4, noises.Item2[i].TileData.d5, noises.Item2[i].TileData.d6, noises.Item2[i].TileData.d7, noises.Item2[i].TileData.d4, noises.Item2[i].TileData.d5, noises.Item2[i].TileData.d6, noises.Item2[i].TileData.d7)].RotationAngle, 0);
                    }
                }
            }
        }
        #endregion

        #region ComputeShader
        public (GridTileGPU[], GridSubtileGPU[]) ShaderGenerateChunk(int ChunkCoordX, int ChunkCoordY, int ChunkCoordZ, Vector2[] mapOctaveOffsets, Vector2[] mapSubOctaveOffsets, MapRegionGPU[] mapRegionGPUs, KeyFrameGPU[] keyFrameGPUs)
        {
            //Run through a chunk of 10x10x10 grid tiles
            int threadNum = 10;

            //Outputs a Chunk of 8x8x8 grid tiles = 512
            GridTileGPU[] gridTileOutputs = new GridTileGPU[512];
            //Outputs a Chunk of 9x9x9 grid subtiles = 729
            GridSubtileGPU[] gridSubtileOutputs = new GridSubtileGPU[729];

            //Find bytes
            int sizeOfGridTiles;
            int sizeOfGridSubtileGPUs;
            int sizeOfMapOctaveOffsets;
            int sizeOfMapRegionGPUs;
            int sizeOfKeyFrameGPUs;
            unsafe
            {
                sizeOfGridTiles = sizeof(GridTileGPU);
                sizeOfGridSubtileGPUs = sizeof(GridSubtileGPU);
                sizeOfMapOctaveOffsets = sizeof(Vector2);
                sizeOfMapRegionGPUs = sizeof(MapRegionGPU);
                sizeOfKeyFrameGPUs = sizeof(KeyFrameGPU);
            }

            //Kernal
            int kernel = WGShader.FindKernel("NoiseOld");

            //RWStructuredBuffer sets
            ComputeBuffer GridTilesBuffer = new ComputeBuffer(gridTileOutputs.Length, sizeOfGridTiles);
            ComputeBuffer GridSubtilesBuffer = new ComputeBuffer(gridSubtileOutputs.Length, sizeOfGridSubtileGPUs);
            GridTilesBuffer.SetData(gridTileOutputs);
            GridSubtilesBuffer.SetData(gridSubtileOutputs);
            WGShader.SetBuffer(kernel, "GridTiles", GridTilesBuffer);
            WGShader.SetBuffer(kernel, "GridSubTiles", GridSubtilesBuffer);

            //Sets
            WGShader.SetInts("ThreadDimensions", new int[3] { threadNum, threadNum, threadNum });
            WGShader.SetInts("ChunkCoord", new int[3] { ChunkCoordX, ChunkCoordY, ChunkCoordZ });
            WGShader.SetFloat("MapScale", MapScale);
            WGShader.SetFloat("MapPersistance", MapPersistance);
            WGShader.SetFloat("MapLacunarity", MapLacunarity);
            WGShader.SetInt("Octaves", MapOctaves);
            WGShader.SetInt("Sites", mapRegionGPUs.Length);
            WGShader.SetInt("Thresholds", keyFrameGPUs.Length);

            //StructuredBuffer sets
            ComputeBuffer mapOctaveOffsetsBuffer = new ComputeBuffer(mapOctaveOffsets.Length, sizeOfMapOctaveOffsets);
            ComputeBuffer mapSubOctaveOffsetsBuffer = new ComputeBuffer(mapSubOctaveOffsets.Length, sizeOfMapOctaveOffsets);
            ComputeBuffer mapRegionGPUsBuffer = new ComputeBuffer(mapRegionGPUs.Length, sizeOfMapRegionGPUs);
            ComputeBuffer keyFrameGPUsBuffer = new ComputeBuffer(keyFrameGPUs.Length, sizeOfKeyFrameGPUs);
            mapOctaveOffsetsBuffer.SetData(mapOctaveOffsets);
            mapSubOctaveOffsetsBuffer.SetData(mapSubOctaveOffsets);
            mapRegionGPUsBuffer.SetData(mapRegionGPUs);
            keyFrameGPUsBuffer.SetData(keyFrameGPUs);
            WGShader.SetBuffer(kernel, "MapOctaveOffsets", mapOctaveOffsetsBuffer);
            WGShader.SetBuffer(kernel, "SubMapOctaveOffsets", mapSubOctaveOffsetsBuffer);
            WGShader.SetBuffer(kernel, "MapRegions", mapRegionGPUsBuffer);
            WGShader.SetBuffer(kernel, "ThresholdFrames", keyFrameGPUsBuffer);

            //Run kernal
            WGShader.Dispatch(kernel, threadNum, threadNum, threadNum);

            //Grab outputs
            GridTilesBuffer.GetData(gridTileOutputs);
            GridSubtilesBuffer.GetData(gridSubtileOutputs);

            //Get rid of buffer data
            GridTilesBuffer.Dispose();
            GridSubtilesBuffer.Dispose();
            mapOctaveOffsetsBuffer.Dispose();
            mapSubOctaveOffsetsBuffer.Dispose();
            mapRegionGPUsBuffer.Dispose();
            keyFrameGPUsBuffer.Dispose();

            return (gridTileOutputs, gridSubtileOutputs);
        }
        #endregion

        #region Setup
        //Creates all pattern varients from the pattern template for a 2x2 box and add it to the dictionary
        void CreateAllPatternVarients()
        {
            for (int i = 0; i < GridPatternTemplates.Count; i++)
            {
                GridPatterns.Add(TileDataGPU.StringToTileDataGPU(GridPatternTemplates[i].PatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, false, 0));

                if (GridPatternTemplates[i].PatternData == "00000000" || GridPatternTemplates[i].PatternData == "11111111") continue;

                //Rotates the pattern by 90 degrees each iteration (90 - 270)
                char[] previousRotatedPatternData = GridPatternTemplates[i].PatternData.ToCharArray();
                float previousRotationAngle = 0;
                for (int j = 0; j < 3; j++)
                {
                    char[] currentRotatedPatternData = new char[previousRotatedPatternData.Length];
                    //Bottom 2x2
                    currentRotatedPatternData[0] = previousRotatedPatternData[3];
                    currentRotatedPatternData[1] = previousRotatedPatternData[0];
                    currentRotatedPatternData[2] = previousRotatedPatternData[1];
                    currentRotatedPatternData[3] = previousRotatedPatternData[2];
                    //Top 2x2
                    currentRotatedPatternData[4] = previousRotatedPatternData[7];
                    currentRotatedPatternData[5] = previousRotatedPatternData[4];
                    currentRotatedPatternData[6] = previousRotatedPatternData[5];
                    currentRotatedPatternData[7] = previousRotatedPatternData[6];

                    previousRotationAngle += 90;

                    TileDataGPU key = TileDataGPU.StringToTileDataGPU(new string(currentRotatedPatternData));

                    if (!GridPatterns.ContainsKey(key))
                    {
                        if (j == 0) GridPatterns.Add(key, new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, true, previousRotationAngle));
                        else if (j == 1) GridPatterns.Add(key, new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, true, previousRotationAngle));
                        else GridPatterns.Add(key, new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, false, previousRotationAngle));
                    }

                    previousRotatedPatternData = currentRotatedPatternData;
                }
            }
        }

        public void GenerateVoronoiGraph(List<MapRegion> outputMapRegions = null, List<List<Vector2>> outputRegions = null, List<LineSegment> outputSpanningTrees = null)
        {
            Rect bounds = new Rect(0, 0, MapSize.x * MapChunkSize, MapSize.y * MapChunkSize);
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

            Vector2 center = new Vector3((MapSize.x * MapChunkSize) * 0.5f, (MapSize.y * MapChunkSize) * 0.5f);
            for (int i = 0; i < VoronoiSiteCount; i++)
            {
                if (VoronoiMaskType == VoronoiGradientMaskType.Circle)
                {
                    Vector2 randomPoint = UnityEngine.Random.insideUnitCircle;
                    points.Add(center + new Vector2(randomPoint.x, randomPoint.y) * (((MapSize.x * MapChunkSize) * 0.5f) * VoronoiMaskRadius));
                }
                else points.Add(new Vector2(Random.Range(0, MapSize.x * MapChunkSize), Random.Range(0, MapSize.y * MapChunkSize)));
            }
            return points;
        }

        public KDTree CreateKDTree(List<MapRegion> mapRegions)
        {
            Vector3[] convertedSiteCoords = new Vector3[mapRegions.Count];
            for (int i = 0; i < convertedSiteCoords.Length; i++) convertedSiteCoords[i] = new Vector3(mapRegions[i].RegionSite.x, 0, mapRegions[i].RegionSite.y);

            return KDTree.MakeFromPoints(convertedSiteCoords);
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
            for(int i = 0; i < mapRegions.Count; i++)
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
