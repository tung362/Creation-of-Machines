﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

namespace COM
{
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

    public enum PlacementType
    {
        Normal,
        Floor,
        Ceiling
    }

    //[System.Serializable]
    //public class HouseFloor
    //{
    //    public int StyleType;
    //    public Dictionary<Vector3Int, GridTile> GridTiles = new Dictionary<Vector3Int, GridTile>();
    //    public Dictionary<Vector3Int, GridSubTile> GridSubTiles = new Dictionary<Vector3Int, GridSubTile>();

    //    public HouseFloor(int newStyleType)
    //    {
    //        StyleType = newStyleType;
    //    }
    //}

    //[System.Serializable]
    //public class GridTile
    //{
    //    public GameObject Tile;
    //    //public HashSet<Vector3Int> AdjacentTiles = new HashSet<Vector3Int>();
    //}

    [System.Serializable]
    public class GridSubTile
    {
        public List<GameObject> Tiles = new List<GameObject>();
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
        public int[] TileTypes;
    }

    [System.Serializable]
    public class GridPatternVarient
    {
        public int[] TileTypes;
        public bool Invert;
        public bool Rotate;
        public float RotationAngle;

        public GridPatternVarient(int[] newTileTypes, bool shouldInvert, bool shouldRotate, float newRotationAngle)
        {
            TileTypes = newTileTypes;
            Invert = shouldInvert;
            Rotate = shouldRotate;
            RotationAngle = newRotationAngle;
        }
    }
}