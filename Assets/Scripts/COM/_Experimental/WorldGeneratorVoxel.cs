using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using COM.Database.World;
using COM.Utils.World;

namespace COM.World.Experimental
{
    /// <summary>
    /// Terrain generator using the voxels model
    /// </summary>
    public class WorldGeneratorVoxel : MonoBehaviour
    {
        public static WorldGeneratorVoxel instance { get; private set; }

        [Header("Editor")]
        public ComputeShader WGEditorShader;

        [Header("Database")]
        public SurfaceBiomeDatabase SurfaceBiomes;
        public CaveBiomeDatabase CaveBiomes;

        [Header("Voronoi Generator")]
        public VoronoiGenerator VoronoiGenerator;

        [Header("Noise Generator")]
        public VoxelNoiseGenerator NoiseGenerator;

        [Header("Voxel Generator")]
        public VoxelGenerator Generator;

        [Header("Generator Settings")]
        public string MapSeed = "Seed";
        public Vector2Int MapSize = new Vector2Int(152, 152);
        [Range(1, 100)]
        public int CubesPerAxis = 30;
        public float CubeSize = 0.0255f;
        public Material TerrainMaterial;

        [Header("Debug")]
        public int ViewSize = 1;
        public GameObject RegionBorderPrefab;
        public Material TestMaterial;
        private List<MeshFilter> TestChunks = new List<MeshFilter>();

        /*Cache*/
        public readonly List<Region> Regions = new List<Region>();
        private GameObject ChunkOrigin;
        private GameObject MiniMapOrigin;

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"WorldGenerator\"");
        }

        void Start()
        {
            //Creates chunk origin gameobject
            ChunkOrigin = new GameObject("Chunks");
            ChunkOrigin.transform.position = Vector3.zero;

            //Creates minimap origin gameobject
            MiniMapOrigin = new GameObject("Minimap");
            MiniMapOrigin.transform.position = Vector3.zero;

            /*Create generation data*/
            VoronoiGenerator.Result(MapSeed, MapSize, Regions);
            RandomizeMapRegions(Regions);

            /*Generator setup*/
            NoiseGenerator.Init(MapSeed, CubesPerAxis, CubeSize, SurfaceBiomes.Biomes, CaveBiomes.Biomes, Regions);
            Generator.Init(CubesPerAxis, CubeSize);

            /*Encode generation data for fragment shader use*/
            TerrainMaterial.SetTexture("_SurfaceRegions", SurfaceRegionEncoder.CreateTexture(SurfaceBiomes.Biomes));
            TerrainMaterial.SetTexture("_CaveRegions", CaveRegionEncoder.CreateTexture(CaveBiomes.Biomes));
            TerrainMaterial.SetTexture("_RegionMap", RegionMapEncoder.CreateTexture(Regions));

            /*Debug*/
            GenerateRegionBorders();

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

            //CreateChunk(0, 1, 0);
            //CreateChunk(0, 0, 0);
            //CreateChunk(0, -1, 0);
            //CreateChunk(0, 1, 1);
            //CreateChunk(0, 0, 1);
            //CreateChunk(0, -1, 1);
        }

        void OnDestroy()
        {
            NoiseGenerator.Dispose();
            Generator.Dispose();
        }

        #region Generation

        void CreateChunk(int ChunkCoordX, int ChunkCoordY, int ChunkCoordZ)
        {
            (Vector3[], int[], FragRegionIndex) meshData = Generator.Result(ChunkCoordX, ChunkCoordY, ChunkCoordZ, CubesPerAxis, NoiseGenerator);
            if (meshData.Item1.Length == 0) return;

            GameObject chunk = new GameObject();
            chunk.transform.parent = ChunkOrigin.transform;
            chunk.transform.localPosition = new Vector3(ChunkCoordX, ChunkCoordY, ChunkCoordZ) * (3 * (CubesPerAxis * CubeSize));
            MeshFilter chunkMF = chunk.AddComponent<MeshFilter>();
            MeshRenderer chunkMR = chunk.AddComponent<MeshRenderer>();
            MeshCollider chunkMC = chunk.AddComponent<MeshCollider>();

            chunkMR.material = TerrainMaterial;
            //chunkMR.material = TestMaterial;

            chunkMR.material.SetInt("_BiomeIndex0", meshData.Item3.Index0);
            chunkMR.material.SetInt("_BiomeIndex1", meshData.Item3.Index1);
            chunkMR.material.SetInt("_BiomeIndex2", meshData.Item3.Index2);
            chunkMR.material.SetInt("_BiomeIndex3", meshData.Item3.Index3);

            Mesh mesh = new Mesh()
            {
                vertices = meshData.Item1,
                triangles = meshData.Item2
            };
            mesh.RecalculateNormals();
            chunkMF.sharedMesh = mesh;
            TestChunks.Add(chunkMF);
        }
        #endregion

        #region Utils
        public void RandomizeMapRegions(List<Region> mapRegions)
        {
            Random.InitState(MapSeed.GetHashCode());

            //Might change this later so that it chooses from a list of presets randomly instead of fully random
            for (int i = 0; i < mapRegions.Count; i++)
            {
                //Generate names
                //Randomizes surface and cave biomes for each region
                mapRegions[i].SurfaceBiomeID = Random.Range(0, SurfaceBiomes.Biomes.Count);
                mapRegions[i].CaveBiomeID = Random.Range(0, CaveBiomes.Biomes.Count);
            }
        }
        #endregion

        #region Debug
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
