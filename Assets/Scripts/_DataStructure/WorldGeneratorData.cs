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
                //ThresholdColors = new Color[LayerThreshold.length];
                //ColorThreshold.keys = new Keyframe[0];
                MaxLayerHeight = 0;

                for (int i = 0; i < LayerThreshold.length; i++)
                {
                    //Generate colors according to number of key frames of the layer threshold
                    //ThresholdColors[i] = Color.HSVToRGB((float)i / (LayerThreshold.length - 1), 1, 1);

                    //Map out color indexs according to the key frames of the layer threshold
                    //Keyframe sampledFrame = LayerThreshold[i];
                    //sampledFrame.value = i;
                    //ColorThreshold.AddKey(sampledFrame);

                    if (MaxLayerHeight < LayerThreshold[i].value) MaxLayerHeight = (int)LayerThreshold[i].value;
                }
            }
        }
        //[Header("Read Only")]
        //[ReadOnly]
        //public AnimationCurve ColorThreshold;
        //[ReadOnly]
        //public Color[] ThresholdColors;

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

    public struct KeyFrameGPU
    {
        public float FrameTime;
        public float FrameValue;
    };

    public struct GridTileGPU
    {
        public int IsEmpty;
        public Vector3Int Coord;
    };

    public struct GridSubtileGPU
    {
        public TileDataGPU TileData;
        public Vector3Int Coord;
    };

    struct TriangleGPU
    {
        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;
    };

    public partial struct TileDataGPU
    {
        public int d0, d1, d2, d3, d4, d5, d6, d7;

        public TileDataGPU(int d0, int d1, int d2, int d3, int d4, int d5, int d6, int d7)
        {
            this.d0 = d0;
            this.d1 = d1;
            this.d2 = d2;
            this.d3 = d3;
            this.d4 = d4;
            this.d5 = d5;
            this.d6 = d6;
            this.d7 = d7;
        }

        public int this[int index]
        {
            get
            {
                switch(index)
                {
                    case 0: return d0;
                    case 1: return d1;
                    case 2: return d2;
                    case 3: return d3;
                    case 4: return d4;
                    case 5: return d5;
                    case 6: return d6;
                    case 7: return d7;
                    default:
                        throw new System.IndexOutOfRangeException("Invalid TileDataGPU index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: d0 = value; break;
                    case 1: d1 = value; break;
                    case 2: d2 = value; break;
                    case 3: d3 = value; break;
                    case 4: d4 = value; break;
                    case 5: d5 = value; break;
                    case 6: d6 = value; break;
                    case 7: d7 = value; break;
                    default:
                        throw new System.IndexOutOfRangeException("Invalid TileDataGPU index!");
                }
            }
        }

        public static TileDataGPU StringToTileDataGPU(string s)
        {
            TileDataGPU tileDataGPU = new TileDataGPU();
            tileDataGPU.d0 = (int)char.GetNumericValue(s[0]);
            tileDataGPU.d1 = (int)char.GetNumericValue(s[1]);
            tileDataGPU.d2 = (int)char.GetNumericValue(s[2]);
            tileDataGPU.d3 = (int)char.GetNumericValue(s[3]);
            tileDataGPU.d4 = (int)char.GetNumericValue(s[4]);
            tileDataGPU.d5 = (int)char.GetNumericValue(s[5]);
            tileDataGPU.d6 = (int)char.GetNumericValue(s[6]);
            tileDataGPU.d7 = (int)char.GetNumericValue(s[7]);
            return tileDataGPU;
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", d0.ToString(format, formatProvider), d1.ToString(format, formatProvider), d2.ToString(format, formatProvider), d3.ToString(format, formatProvider), d4.ToString(format, formatProvider), d5.ToString(format, formatProvider), d6.ToString(format, formatProvider), d7.ToString(format, formatProvider));
        }

        public static bool operator ==(TileDataGPU lhs, TileDataGPU rhs)
        {
            return lhs.d0 == rhs.d0 &&
                   lhs.d1 == rhs.d1 &&
                   lhs.d2 == rhs.d2 &&
                   lhs.d3 == rhs.d3 &&
                   lhs.d4 == rhs.d4 &&
                   lhs.d5 == rhs.d5 &&
                   lhs.d6 == rhs.d6 &&
                   lhs.d7 == rhs.d7;
        }

        public static bool operator !=(TileDataGPU lhs, TileDataGPU rhs)
        {
            return !(lhs == rhs);
        }
    }

    [System.Serializable]
    public class GameObjectPool
    {
        public HashSet<GameObject> Used = new HashSet<GameObject>();
        public HashSet<GameObject> UnUsed = new HashSet<GameObject>();
    }
}