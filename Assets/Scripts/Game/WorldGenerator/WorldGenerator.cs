using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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
    public GenerationGradientType GradientType = GenerationGradientType.None;
    [Range(-2.0f, 2.0f)]
    public float GradientRadius = 0.75f;

    void Start()
    {

    }

    void Update()
    {
    }

    public float GenerateMapCoordData(Vector2 tileCoord, Vector2[] mapOctavesOffsets)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float totalAmplitude = 0;  // Used for normalizing result to 0.0 - 1.0

        for (int i = 0; i < mapOctavesOffsets.Length; i++)
        {
            float mapX = tileCoord.x / MapScale * frequency + mapOctavesOffsets[i].x;
            float mapY = tileCoord.y / MapScale * frequency + mapOctavesOffsets[i].y;

            total += Mathf.PerlinNoise(mapX, mapY) * amplitude;
            totalAmplitude += amplitude;

            amplitude *= MapPersistance;
            frequency *= MapLacunarity;

            //total += Mathf.PerlinNoise((tileCoord.x + MapOffset.x) / MapScale * frequency, (tileCoord.y + MapOffset.y) / MapScale * frequency) * amplitude;
            //totalAmplitude += amplitude;
            //amplitude *= MapPersistance;
            //frequency *= MapLacunarity;
        }
        //if (total / totalAmplitude > 1 || total / totalAmplitude < 0) Debug.Log(total / totalAmplitude);
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

    float GradientNoise(int X, int Y, int Size, float Radius, float heightValue, int GradientType)
    {
        //Smooth circle and square
        if (GradientType == 0 || GradientType == 1)
        {
            //Gradient
            float distanceX = Mathf.Abs(X - Size * 0.5f);
            float distanceY = Mathf.Abs(Y - Size * 0.5f);

            //Circular mask
            float distance = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

            //square mask
            if (GradientType == 1) distance = Mathf.Max(distanceX, distanceY);

            float maxWidth = Size * Radius - 10.0f;
            float delta = distance / maxWidth;
            float gradient = delta * delta;

            heightValue *= Mathf.Max(0.0f, 1.0f - gradient);
        }
        return heightValue;
    }
}
