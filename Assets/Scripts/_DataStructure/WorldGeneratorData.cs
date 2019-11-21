using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using csDelaunay;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using System;

namespace COM
{
    public enum MapGradientMaskType { None, Circle, Square }
    public enum VoronoiGradientMaskType { None, Circle }
    public enum VoronoiBiomeMaskType { None, Radial, ThickBorders, ThinBorders };

    [System.Serializable]
    public struct GenerationThreshold
    {
        public AnimationCurve _LayerThreshold;
        public AnimationCurve LayerThreshold
        {
            get { return _LayerThreshold; }
            set
            {
                _LayerThreshold = value;
                MaxLayerHeight = 0;

                for (int i = 0; i < LayerThreshold.length; i++)
                {
                    if (MaxLayerHeight < LayerThreshold[i].value) MaxLayerHeight = (int)LayerThreshold[i].value;
                }
            }
        }

        [ReadOnly]
        public int MaxLayerHeight;
    }

    [System.Serializable]
    public class MapRegion
    {
        public string RegionName;
        public int FactionID;
        public int RegionType;
        public int CaveRegionType;
        public Site RegionSite;

        public float GenerationModifier;
        public float GenerationCaveModifier;
    }

    public struct MapRegionGPU
    {
        public int RegionType;
        public int CaveRegionType;
        public Vector2 Coord;

        public float GenerationModifier;
        public float GenerationCaveModifier;
    };

    public struct WGEditorOutputGPU
    {
        public Vector2Int coord;
        public float voronoiHeight;
        public float normHeight;
        public float voronoiMatchHeight;
        public float combinedHeight;
        public float subHeight;
        public float subVoronoiMatchHeight;
        public float subCombinedHeight;
    };

    struct TriangleGPU
    {
        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;
    };
}