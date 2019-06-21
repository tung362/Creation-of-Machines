using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainDecorationData
{
    public int MinSpawn = 0;
    public int MaxSpawn = 1;
    public List<GameObject> Variants = new List<GameObject>();
}
