using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

namespace COM.World
{
    /// <summary>
    /// Voronoi generator for creating map regions
    /// </summary>
    [System.Serializable]
    public class VoronoiGenerator
    {
        #region Format
        public enum VoronoiGradientMaskType { None, Circle }
        #endregion

        [Header("General Settings")]
        [Range(1, 100)]
        public int VoronoiSiteCount = 100;
        public int VoronoiRelaxationCount = 5;
        public VoronoiGradientMaskType VoronoiMaskType = VoronoiGradientMaskType.Circle;
        [Range(0.01f, 1)]
        public float VoronoiMaskRadius = 0.75f;

        #region Output
        public void Result(string mapSeed, Vector2Int mapSize, List<Region> outputMapRegions = null, List<List<Vector2>> outputRegions = null, List<LineSegment> outputSpanningTrees = null)
        {
            Rect bounds = new Rect(0, 0, mapSize.x, mapSize.y);
            List<Vector2> points = CreateRandomPoints(mapSeed, mapSize);

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
        #endregion

        #region Utils
        public List<Vector2> CreateRandomPoints(string mapSeed, Vector2Int mapSize)
        {
            Random.InitState(mapSeed.GetHashCode());
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
        #endregion
    }
}
