#ifndef VoronoiPoints_
#define VoronoiPoints_

inline float2 RandomVector(float2 uv, float offset)
{
    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
    uv = frac(sin(mul(uv, m)) * 46839.32);
    return float2(sin(uv.y * + offset) * 0.5 + 0.5, cos(uv.x * offset) * 0.5 + 0.5);
}

void VoronoiPoints_float(float2 uv, float angleOffset, float cellDensity, out float2 cells)
{
    float worldOffset = 1.0f / cellDensity;
    float2 sampleGrid = floor(uv * cellDensity);
    float2 sampleCoord = frac(uv * cellDensity);
    float closestDistance = 999999;
    float2 localSite;
    float2 localGridOffset;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 gridOffset = float2(x, y);
            float2 siteOffset = RandomVector(gridOffset + sampleGrid, angleOffset);
            float2 Site = gridOffset + siteOffset;
            float dist = distance(Site, sampleCoord);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                localSite = siteOffset;
                localGridOffset = gridOffset;
                //cells = (sampleCoord - Site) + 0.5f;
            }
        }
    }
    float2 worldGrid = (sampleGrid + localGridOffset) / cellDensity;
    cells = saturate(worldGrid + (localSite * worldOffset));
}
#endif