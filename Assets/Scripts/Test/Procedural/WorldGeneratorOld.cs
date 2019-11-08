using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

namespace COM.Test
{
    //handles all of the world's procedural generation
    public class WorldGeneratorOld : MonoBehaviour
    {
        public static WorldGeneratorOld instance { get; private set; }

        [Header("Generator Settings")]
        public ComputeShader CalculationsShader;
        public string MapSeed = "Seed";
        public Vector2Int MapSize = new Vector2Int(100, 100);
        public int MapChunkSize = 16;
        public float MapScale = 20;
        public int MapOctaves = 4;
        public float MapPersistance = 0.5f;
        public float MapLacunarity = 1.87f;
        public Vector2 MapOffset = Vector2.zero;
        public MapGradientMaskType MapMaskType = MapGradientMaskType.None;
        [Range(-2.0f, 2.0f)]
        public float MapMaskRadius = 0.75f;
        public GenerationThreshold MapLayers;

        [Header("Voronoi Settings")]
        public int VoronoiSiteCount = 100;
        public int VoronoiRelaxationCount = 5;
        public VoronoiGradientMaskType VoronoiMaskType = VoronoiGradientMaskType.None;
        [Range(0.01f, 1)]
        public float VoronoiMaskRadius = 1.0f;
        public VoronoiBiomeMaskType VoronoiBiomeMaskType = VoronoiBiomeMaskType.None;

        [Header("Builder Settings")]
        //Stores all unique patterns
        public List<GridPattern> GridPatternTemplates = new List<GridPattern>();
        public float SnapOffset = 0.68f;
        public GameObject GridTilePrefab;
        public GameObject TestObject;

        /*Generation Data*/
        public List<MapRegion> MapRegions { get; private set; }
        public KDTree VoronoiKDTree { get; private set; }
        public Vector2[] MapOctaveOffsets { get; private set; }
        public Vector2[] SubMapOctaveOffsets { get; private set; }
        public float MapChunkOffset { get; private set; }
        public Vector2 Center { get; private set; }

        /*Builder Data*/
        //Stores all tiles used the house
        private Dictionary<Vector3Int, GameObject> GridTiles = new Dictionary<Vector3Int, GameObject>();
        private Dictionary<Vector3Int, GridSubTile> GridSubTiles = new Dictionary<Vector3Int, GridSubTile>();
        //Stores all possible varients of each pattern
        private Dictionary<string, GridPatternVarient> GridPatterns = new Dictionary<string, GridPatternVarient>();
        //Grid origin and parent object for the house
        private GameObject GridOrigin;

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"WorldGenerator\"");
        }

        void Start()
        {
            //Init
            MapRegions = new List<MapRegion>();
            MapChunkOffset = SnapOffset * MapChunkSize;
            GridOrigin = new GameObject("Terrain");
            GridOrigin.transform.position = Vector3.zero;

            GenerateMap();
            CreateAllPatternVarients();
            //Hover(new Vector3Int(0, 0, 0), true);
            //Hover(new Vector3Int(16, 0, 0), true);
            //Hover(new Vector3Int(0, 0, 16), true);
            //Hover(new Vector3Int(16, 0, 16), true);

            //Hover(new Vector3Int(0, 0, 0), true);
            //Hover(new Vector3Int(16, 0, 0), true);
            //Hover(new Vector3Int(0, 0, 16), true);
            //Hover(new Vector3Int(16, 0, 16), true);

            //GenerateMapChunk(0, 0);
            //GenerateMapChunk(1, 0);
            //GenerateMapChunk(-1, 0);
            //GenerateMapChunk(0, 1);
            //GenerateMapChunk(0, -1);
            //GenerateMapChunk(1, 1);
            //GenerateMapChunk(-1, -1);
            //GenerateMapChunk(-1, 1);
            //GenerateMapChunk(1, -1);
            //int[] outputRegionIndexes;
            //GenerateVoronoiMapCoordData2(new Vector2(10, 0), MapRegions, VoronoiKDTree, out outputRegionIndexes);
        }

        void Update()
        {

        }

        #region Compute Shader Calculation
        //public void GenerateVoronoiMapCoordData2(Vector2 tileCoord, List<MapRegion> mapRegions, KDTree tree, out int[] outputRegionIndexes)
        //{
        //    outputRegionIndexes = tree.FindNearestsK(new Vector3(tileCoord.x, 0, tileCoord.y), 3);

        //    //Inputs
        //    Vector2 TileCoord = tileCoord;
        //    Vector2 ClosestSiteCoord = mapRegions[outputRegionIndexes[0]].RegionSite.Coord;
        //    Vector2 SecondClosestSiteCoord = mapRegions[outputRegionIndexes[1]].RegionSite.Coord;
        //    int MaskType = (int)VoronoiBiomeMaskType;

        //    //Outputs
        //    float[] Result = new float[1];

        //    //Dynamic way to figure out data sizes
        //    int sizeOfTileCoord;
        //    int sizeOfClosestSiteCoord;
        //    int sizeOfSecondClosestSiteCoord;
        //    int sizeOfMaskType;
        //    int sizeOfResult;
        //    unsafe
        //    {
        //        sizeOfTileCoord = sizeof(Vector2);
        //        sizeOfClosestSiteCoord = sizeof(Vector2);
        //        sizeOfSecondClosestSiteCoord = sizeof(Vector2);
        //        sizeOfMaskType = sizeof(int);
        //        sizeOfResult = sizeof(float);
        //    }

        //    //identify kernal
        //    int kernel = CalculationsShader.FindKernel("Uka");

        //    //Property define for shader
        //    ComputeBuffer resultBuffer = new ComputeBuffer(1, sizeOfResult);
        //    //ComputeBuffer float3Buffer = new ComputeBuffer(input.Length, sizeOfClosestSiteCoord);
        //    //ComputeBuffer float3Buffer = new ComputeBuffer(input.Length, sizeOfSecondClosestSiteCoord);
        //    //ComputeBuffer float3Buffer = new ComputeBuffer(input.Length, sizeOfMaskType);
        //    //Apply type and default value to buffer
        //    resultBuffer.SetData(Result);
        //    //Target shader property to assign value the buffer value to
        //    CalculationsShader.SetBuffer(kernel, "Result", resultBuffer);

        //    CalculationsShader.SetVector("TileCoord", TileCoord);
        //    CalculationsShader.SetVector("ClosestSiteCoord", ClosestSiteCoord);
        //    CalculationsShader.SetVector("SecondClosestSiteCoord", SecondClosestSiteCoord);
        //    CalculationsShader.SetInt("MaskType", MaskType);

        //    //Run shader
        //    CalculationsShader.Dispatch(kernel, 2, 1, 1);

        //    //Output data
        //    resultBuffer.GetData(Result);

        //    Debug.Log(Result[0]);

        //    //GC
        //    resultBuffer.Dispose();
        //}
        #endregion

        #region Generation
        void GenerateMap()
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

            //Map center
            Center = new Vector2((float)MapSize.x / 2, (float)MapSize.y / 2);
        }

        public void CreateMapChunk(int ChunkCoordX, int ChunkCoordY)
        {
            //Debug.Log("Create");
            //Modify texture
            for (int x = ChunkCoordX * MapChunkSize; x < (ChunkCoordX * MapChunkSize) + MapChunkSize; x++)
            {
                for (int z = ChunkCoordY * MapChunkSize; z < (ChunkCoordY * MapChunkSize) + MapChunkSize; z++)
                {
                    //Voronoi map
                    List<int> regionIndexes = new List<int>();
                    float voronoiHeight = GenerateVoronoiMapCoordData(new Vector2(x, z), MapRegions, VoronoiKDTree, regionIndexes);

                    //Normal map
                    float normHeight = GenerateMapCoordData(new Vector2(x - Center.x, z - Center.y), MapOctaveOffsets);
                    float combinedHeight = BiomeHeightBlend(normHeight, voronoiHeight, new Vector2(x, z), regionIndexes, MapRegions, false);

                    //Subtractive map
                    float subHeight = GenerateMapCoordData(new Vector2(x - Center.x, z - Center.y), SubMapOctaveOffsets);
                    float subCombinedHeight = BiomeHeightBlend(subHeight, voronoiHeight, new Vector2(x, z), regionIndexes, MapRegions, true);

                    for(int y = 0; y < MapLayers.LayerThreshold.Evaluate(combinedHeight); y++)
                    {
                        Hover(new Vector3Int(x, y, z), true);
                    }

                    for (int y = 0; y < MapLayers.LayerThreshold.Evaluate(subCombinedHeight); y++)
                    {
                        Hover(new Vector3Int(x, y, z), false);
                    }
                }
            }
        }

        public void DestroyMapChunk(int ChunkCoordX, int ChunkCoordY)
        {
            //Debug.Log("Remove");
            //Modify texture
            for (int x = ChunkCoordX * MapChunkSize; x < (ChunkCoordX * MapChunkSize) + MapChunkSize; x++)
            {
                for (int z = ChunkCoordY * MapChunkSize; z < (ChunkCoordY * MapChunkSize) + MapChunkSize; z++)
                {
                    //Voronoi map
                    List<int> regionIndexes = new List<int>();
                    float voronoiHeight = GenerateVoronoiMapCoordData(new Vector2(x, z), MapRegions, VoronoiKDTree, regionIndexes);

                    //Normal map
                    float normHeight = GenerateMapCoordData(new Vector2(x - Center.x, z - Center.y), MapOctaveOffsets);
                    float combinedHeight = BiomeHeightBlend(normHeight, voronoiHeight, new Vector2(x, z), regionIndexes, MapRegions, false);

                    for (int y = 0; y < MapLayers.LayerThreshold.Evaluate(combinedHeight); y++)
                    {
                        Hover(new Vector3Int(x, y, z), false);
                    }
                }
            }
        }

        public float BiomeHeightBlend(float baseHeight, float voronoiBaseHeight, Vector2 coord, List<int> regionIndexes, List<MapRegion> mapRegions, bool isSubtractive)
        {
            float result = baseHeight * Mathf.Lerp(1, isSubtractive ? mapRegions[regionIndexes[0]].GenerationCaveModifier : mapRegions[regionIndexes[0]].GenerationModifier, voronoiBaseHeight);

            //If the closest region's type is the same as the second closest region's type
            if ((isSubtractive ? mapRegions[regionIndexes[0]].CaveRegionType : mapRegions[regionIndexes[0]].RegionType) == (isSubtractive ? mapRegions[regionIndexes[1]].CaveRegionType : mapRegions[regionIndexes[1]].RegionType))
            {
                result = baseHeight * (isSubtractive ? mapRegions[regionIndexes[0]].GenerationCaveModifier : mapRegions[regionIndexes[0]].GenerationModifier);

                //If the closest region's type is the same as the third closest region's type
                if ((isSubtractive ? mapRegions[regionIndexes[0]].CaveRegionType : mapRegions[regionIndexes[0]].RegionType) != (isSubtractive ? mapRegions[regionIndexes[2]].CaveRegionType : mapRegions[regionIndexes[2]].RegionType))
                {
                    float closestDistance = Vector2.Distance(mapRegions[regionIndexes[0]].RegionSite.Coord, coord);
                    float thirdClosestDistance = Vector2.Distance(mapRegions[regionIndexes[2]].RegionSite.Coord, coord);
                    float matchBlend = 1 - (closestDistance / thirdClosestDistance);

                    result = baseHeight * Mathf.Lerp(1, isSubtractive ? mapRegions[regionIndexes[0]].GenerationCaveModifier : mapRegions[regionIndexes[0]].GenerationModifier, matchBlend);
                }
            }
            return result;
        }
        #endregion

        #region Normal Map
        public float GenerateMapCoordData(Vector2 tileCoord, Vector2[] mapOctavesOffsets)
        {
            //Height value
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float totalAmplitude = 0;

            for (int i = 0; i < mapOctavesOffsets.Length; i++)
            {
                float mapX = tileCoord.x / MapScale * frequency + mapOctavesOffsets[i].x;
                float mapY = tileCoord.y / MapScale * frequency + mapOctavesOffsets[i].y;

                total += Mathf.PerlinNoise(mapX, mapY) * amplitude;
                totalAmplitude += amplitude;

                amplitude *= MapPersistance;
                frequency *= MapLacunarity;
            }
            return total / totalAmplitude;
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

        public float GradientNoise(int x, int y, int size, float radius, float heightValue, int gradientType)
        {
            //Smooth circle and square
            if (gradientType == 0 || gradientType == 1)
            {
                //Gradient
                float distanceX = Mathf.Abs(x - size * 0.5f);
                float distanceY = Mathf.Abs(y - size * 0.5f);

                //Circular mask
                float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

                //square mask
                if (gradientType == 1) distance = Mathf.Max(distanceX, distanceY);

                float maxWidth = size * radius - 10.0f;
                float delta = distance / maxWidth;
                float gradient = delta * delta;

                heightValue *= Mathf.Max(0.0f, 1.0f - gradient);
            }
            return heightValue;
        }
        #endregion

        #region Voronoi Map
        public float GenerateVoronoiMapCoordData(Vector2 tileCoord, List<MapRegion> mapRegions, KDTree tree, List<int> outputRegionIndexes = null)
        {
            int[] nearestKs = tree.FindNearestsK(new Vector3(tileCoord.x, 0, tileCoord.y), 3);
            Vector2 closestSiteCoord = mapRegions[nearestKs[0]].RegionSite.Coord;
            Vector2 secondClosestSiteCoord = mapRegions[nearestKs[1]].RegionSite.Coord;
            float closestDistance = Vector2.Distance(closestSiteCoord, tileCoord);
            float secondClosestDistance = Vector2.Distance(secondClosestSiteCoord, tileCoord);

            if (outputRegionIndexes != null)
            {
                outputRegionIndexes.Add(nearestKs[0]);
                outputRegionIndexes.Add(nearestKs[1]);
                outputRegionIndexes.Add(nearestKs[2]);
            }

            if (VoronoiBiomeMaskType == VoronoiBiomeMaskType.Radial) return 1 - Mathf.InverseLerp(0, 20, closestDistance);
            else if (VoronoiBiomeMaskType == VoronoiBiomeMaskType.ThickBorders) return 1 - (closestDistance / secondClosestDistance);
            else if (VoronoiBiomeMaskType == VoronoiBiomeMaskType.ThinBorders)
            {
                Vector2 secondClosestDireciton = (secondClosestSiteCoord - closestSiteCoord).normalized;
                Vector2 edgeDirection = Vector2.Perpendicular(secondClosestDireciton);
                Vector2 midPoint = (closestSiteCoord + secondClosestSiteCoord) * 0.5f;

                Vector2 result = GameTools.GetNearestPointOnLine(midPoint, edgeDirection, tileCoord);
                float resultDistance = Vector2.Distance(tileCoord, result);

                //return resultDistance / (secondClosestDistance * 0.5f);
                return resultDistance / 20;
            }
            return 0;
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
                        mapRegions[i].GenerationModifier = 1.0f;
                    }
                    else
                    {
                        mapRegions[i].CaveRegionType = 0;
                        mapRegions[i].GenerationCaveModifier = 1.0f;
                    }
                }
            }
        }
        #endregion

        #region Builder
        //Creates all pattern varients from the pattern template for a 2x2 box and add it to the dictionary
        void CreateAllPatternVarients()
        {
            for (int i = 0; i < GridPatternTemplates.Count; i++)
            {
                GridPatterns.Add(GridPatternTemplates[i].PatternData, new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, false, 0));

                //To do: string is not a value type
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

                    if (!GridPatterns.ContainsKey(new string(currentRotatedPatternData)))
                    {
                        if (j == 0) GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, true, previousRotationAngle));
                        else if (j == 1) GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, true, previousRotationAngle));
                        else GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, false, previousRotationAngle));
                    }

                    previousRotatedPatternData = currentRotatedPatternData;
                }
            }
        }

        void Hover(Vector3Int tileCoord, bool add)
        {
            //Add
            UpdateGridTile(add, tileCoord * 2);
        }

        void UpdateGridTile(bool add, Vector3Int tileCoord)
        {
            if (add)
            {
                //Creates a new grid tile if it doesn't exist already and spawns a new grid tile into the game
                if (!GridTiles.ContainsKey(tileCoord))
                {
                    ////GameObject placementTile = Instantiate(GridTilePrefab, Vector3.zero, Quaternion.identity, GridOrigin.transform);
                    //GameObject placementTile = WorldGeneratorPool.instance.TakeFromPool(GridTilePrefab, GridOrigin.transform);
                    //placementTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
                    //placementTile.transform.localRotation = Quaternion.identity;

                    //GridTiles.Add(tileCoord, placementTile);
                    GridTiles.Add(tileCoord, null);
                }
            }
            else
            {
                if(GridTiles.ContainsKey(tileCoord))
                {
                    //Destroys grid tile
                    ////if (GridTiles[tileCoord]) Destroy(GridTiles[tileCoord]);
                    //if (GridTiles[tileCoord]) WorldGeneratorPool.instance.GiveBackToPool(GridTiles[tileCoord]);
                    GridTiles.Remove(tileCoord);
                }
            }

            //Update grid sub tiles
            //Top 2x2
            UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, 1, 1), new int[] { 2 });
            UpdateGridSubTile(add, tileCoord + new Vector3Int(1, 1, 1), new int[] { 3 });
            UpdateGridSubTile(add, tileCoord + new Vector3Int(1, 1, -1), new int[] { 0 });
            UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, 1, -1), new int[] { 1 });
            //Bottom 2x2
            UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, -1, 1), new int[] { 6 });
            UpdateGridSubTile(add, tileCoord + new Vector3Int(1, -1, 1), new int[] { 7 });
            UpdateGridSubTile(add, tileCoord + new Vector3Int(1, -1, -1), new int[] { 4 });
            UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, -1, -1), new int[] { 5 });
        }

        void UpdateGridSubTile(bool add, Vector3Int tileCoord, int[] affectedTileDataIndexs)
        {
            //Creates a new grid sub tile if it doesn't exist
            if (!GridSubTiles.ContainsKey(tileCoord))
            {
                if (add) GridSubTiles.Add(tileCoord, new GridSubTile("00000000"));
                else return;
            }

            //Edits tile data
            if (affectedTileDataIndexs.Length != 0)
            {
                char[] tileData = GridSubTiles[tileCoord].TileData.ToCharArray();
                for (int i = 0; i < affectedTileDataIndexs.Length; i++) tileData[affectedTileDataIndexs[i]] = add ? '1' : '0';
                GridSubTiles[tileCoord].TileData = new string(tileData);
            }

            //Destroy old sub tiles if it exists
            for (int i = 0; i < GridSubTiles[tileCoord].Tiles.Count; i++)
            {
                ////if (GridSubTiles[tileCoord].Tiles[i]) Destroy(GridSubTiles[tileCoord].Tiles[i]);
                //if(GridSubTiles[tileCoord].Tiles[i]) WorldGeneratorPool.instance.GiveBackToPool(GridSubTiles[tileCoord].Tiles[i]);
            }
            GridSubTiles[tileCoord].Tiles.Clear();

            //If the grid sub tile is no longer used, destroy it's tile and remove from the dictionary
            if (GridSubTiles[tileCoord].TileData == "00000000") GridSubTiles.Remove(tileCoord);
            else
            {
                //Spawns/updates new sub tiles into the game
                if (GridPatterns.ContainsKey(GridSubTiles[tileCoord].TileData))
                {
                    for (int i = 0; i < GridPatterns[GridSubTiles[tileCoord].TileData].TileTypes.Length; i++)
                    {
                        ////GameObject outerSubTile = Instantiate(Run.instance.OuterTiles[new OuterHouseTilesIndex(0, GridPatterns[GridSubTiles[tileCoord].TileData].TileTypes[i])].TileVariants[0], Vector3.zero, Quaternion.identity, GridOrigin.transform);
                        //GameObject outerSubTile = WorldGeneratorPool.instance.TakeFromPool(Run.instance.OuterTiles[new OuterHouseTilesIndex(0, GridPatterns[GridSubTiles[tileCoord].TileData].TileTypes[i])].TileVariants[0], GridOrigin.transform);
                        //outerSubTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
                        //outerSubTile.transform.localEulerAngles = new Vector3(0, i == 0 ? GridPatterns[GridSubTiles[tileCoord].TileData].RotationAngle : GridPatterns[GridSubTiles[tileCoord].TileData.Substring(4, 4) + GridSubTiles[tileCoord].TileData.Substring(4, 4)].RotationAngle, 0);

                        //GridSubTiles[tileCoord].Tiles.Add(outerSubTile);
                    }
                }
                else Debug.Log("Warning! Tile data: " + GridSubTiles[tileCoord].TileData + " is not registered @UpdateGridSubTile()");
            }
        }

        //Run before calling FinalizeBuild()
        public void PreFinalizeBuild()
        {

        }

        //Run after calling PreFinalizeBuild();
        public void FinalizeBuild()
        {

        }
        #endregion
    }
}
