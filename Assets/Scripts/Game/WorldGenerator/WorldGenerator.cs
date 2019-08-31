using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

//[ExecuteInEditMode]
public class WorldGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    //Stores all unique patterns
    public List<GridPattern> GridPatternTemplates = new List<GridPattern>();
    public string MapSeed = "Seed";
    public Vector2Int MapSize = new Vector2Int(100, 100);
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

    void Start()
    {
    }

    void Update()
    {
    }

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

    public Vector2[] GetMapOctaveOffsets()
    {
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

        if(outputRegionIndexes != null)
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

    public KDTree CreateKDTree(List<MapRegion> mapRegions)
    {
        Vector3[] convertedSiteCoords = new Vector3[mapRegions.Count];
        for (int i = 0; i < convertedSiteCoords.Length; i++) convertedSiteCoords[i] = new Vector3(mapRegions[i].RegionSite.x, 0, mapRegions[i].RegionSite.y);

        return KDTree.MakeFromPoints(convertedSiteCoords);
    }

    public void RandomizeMapRegions(ref List<MapRegion> mapRegions)
    {
        //Might change this later so that it chooses from a list of presets randomly instead of fully random
        for(int i = 0; i < mapRegions.Count; i++)
        {
            //Generate names
            //Generate modifiers
            int chance = Random.Range(0, 2);
            if(chance == 0)
            {
                mapRegions[i].RegionType = 1;
                mapRegions[i].GenerationModifier = 2.0f;
                mapRegions[i].GenerationCaveModifier = 1.0f;
            }
            else
            {
                mapRegions[i].RegionType = 0;
                mapRegions[i].GenerationModifier = 0.0f;
                mapRegions[i].GenerationCaveModifier = 1.0f;
            }
        }
    }
    #endregion
}
