using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;


[System.Serializable]
public struct OuterHouseTilesIndex
{
    public int StyleType;
    public int TileType;

    public OuterHouseTilesIndex(int newStyleType, int newTileType)
    {
        StyleType = newStyleType;
        TileType = newTileType;
    }
}

[System.Serializable]
public class OuterTileVarient
{
    public List<GameObject> TileVariants;
}

[System.Serializable]
public class OuterTileDictionary : SerializableDictionaryBase<OuterHouseTilesIndex, OuterTileVarient> { }

public enum OuterHouseStyle
{
    DecayingWood,
    TrimmedWood
}

public enum OuterHouseTileType
{
    Full,
    Corner,
    Side,
    InnerCorner
}

[System.Serializable]
public class HouseFloor
{
    public int StyleType;
    public Dictionary<Vector3Int, GridTile> GridTiles = new Dictionary<Vector3Int, GridTile>();
    public Dictionary<Vector3Int, GridSubTile> GridSubTiles = new Dictionary<Vector3Int, GridSubTile>();

    public HouseFloor(int newStyleType)
    {
        StyleType = newStyleType;
    }
}

[System.Serializable]
public class GridTile
{
    public GameObject Tile;
    public HashSet<Vector3Int> AdjacentTiles = new HashSet<Vector3Int>();
}

[System.Serializable]
public class GridSubTile
{
    public GameObject Tile;
    public string TileData;

    public GridSubTile(string newTileData)
    {
        TileData = newTileData;
    }
}

[System.Serializable]
public class GridPattern
{
    public string PatternData;
    public int TileType;
}

[System.Serializable]
public class GridPatternVarient
{
    public int TileType;
    public bool Invert;
    public bool Rotate;

    public GridPatternVarient(int newTileType, bool shouldInvert, bool shouldRotate)
    {
        TileType = newTileType;
        Invert = shouldInvert;
        Rotate = shouldRotate;
    }
}