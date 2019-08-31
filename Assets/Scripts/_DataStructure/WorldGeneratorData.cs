using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using csDelaunay;

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
    public Site RegionSite;

    public float GenerationModifier;
    public float GenerationCaveModifier;
}