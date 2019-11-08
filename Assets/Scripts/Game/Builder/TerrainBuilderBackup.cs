using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM
{
    //public class TerrainBuilderBackup : MonoBehaviour
    //{
    //    public LayerMask Selection;
    //    //Stores all unique patterns
    //    public List<GridPattern> GridPatternTemplates = new List<GridPattern>();
    //    public int OuterStyleType = 0;
    //    public float SnapOffset = 0.68f;
    //    public GameObject tileTest;
    //    public GameObject tilePrefabTest;

    //    //Stores all tiles used by each floor of the house
    //    private Dictionary<int, HouseFloor> HouseFloors = new Dictionary<int, HouseFloor>();
    //    //Stores all possible varients of each pattern
    //    private Dictionary<string, GridPatternVarient> GridPatterns = new Dictionary<string, GridPatternVarient>();

    //    private HouseFloor CurrentFloor;

    //    //Grid origin and parent object for the house
    //    private GameObject GridOrigin;

    //    private HashSet<Vector3Int> VisitedTiles = new HashSet<Vector3Int>();
    //    private HashSet<Vector3Int> UngroundedTiles = new HashSet<Vector3Int>();

    //    void Start()
    //    {
    //        CreateAllPatternVarients();
    //    }

    //    void Update()
    //    {
    //        Hover();
    //    }

    //    //Creates all pattern varients from the pattern template for a 2x2 box and add it to the dictionary
    //    void CreateAllPatternVarients()
    //    {
    //        for (int i = 0; i < GridPatternTemplates.Count; i++)
    //        {
    //            GridPatterns.Add(GridPatternTemplates[i].PatternData, new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, false, 0));

    //            if (GridPatternTemplates[i].PatternData == "00000000" || GridPatternTemplates[i].PatternData == "11111111") continue;

    //            //Rotates the pattern by 90 degrees each iteration (90 - 270)
    //            char[] previousRotatedPatternData = GridPatternTemplates[i].PatternData.ToCharArray();
    //            float previousRotationAngle = 0;
    //            for (int j = 0; j < 3; j++)
    //            {
    //                char[] currentRotatedPatternData = new char[previousRotatedPatternData.Length];
    //                //Bottom 2x1
    //                currentRotatedPatternData[0] = previousRotatedPatternData[3];
    //                currentRotatedPatternData[1] = previousRotatedPatternData[0];
    //                currentRotatedPatternData[2] = previousRotatedPatternData[1];
    //                currentRotatedPatternData[3] = previousRotatedPatternData[2];
    //                //Top 2x1
    //                currentRotatedPatternData[4] = previousRotatedPatternData[7];
    //                currentRotatedPatternData[5] = previousRotatedPatternData[4];
    //                currentRotatedPatternData[6] = previousRotatedPatternData[5];
    //                currentRotatedPatternData[7] = previousRotatedPatternData[6];

    //                previousRotationAngle += 90;

    //                //Create an inverted version for any invertible possibilities (saves time from having to create a new tile data pattern entry for the inverted version)
    //                //char[] currentInvertedRotatedPatternData = new char[currentRotatedPatternData.Length];
    //                ////Bottom 2x1
    //                //currentInvertedRotatedPatternData[0] = currentRotatedPatternData[1];
    //                //currentInvertedRotatedPatternData[1] = currentRotatedPatternDddata[0];
    //                //currentInvertedRotatedPatternData[2] = currentRotatedPatternData[3];
    //                //currentInvertedRotatedPatternData[3] = currentRotatedPatternData[2];
    //                ////Top 2x1
    //                //currentInvertedRotatedPatternData[4] = currentRotatedPatternData[5];
    //                //currentInvertedRotatedPatternData[5] = currentRotatedPatternData[4];
    //                //currentInvertedRotatedPatternData[6] = currentRotatedPatternData[7];
    //                //currentInvertedRotatedPatternData[7] = currentRotatedPatternData[6];


    //                if (!GridPatterns.ContainsKey(new string(currentRotatedPatternData)))
    //                {
    //                    if (j == 0) GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, true, previousRotationAngle));
    //                    else if (j == 1) GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, true, previousRotationAngle));
    //                    else GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, false, previousRotationAngle));
    //                    //GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, true, previousRotationAngle));
    //                }

    //                //if (!GridPatterns.ContainsKey(new string(currentInvertedRotatedPatternData)))
    //                //{
    //                //    GridPatterns.Add(new string(currentInvertedRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, true, previousRotationAngle - 180.0f));
    //                //}

    //                previousRotatedPatternData = currentRotatedPatternData;
    //            }
    //        }
    //    }

    //    void Hover()
    //    {
    //        //Hovers over tiles and the ground for selection 
    //        RaycastHit hit;
    //        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, Selection))
    //        {
    //            //Blueprint placement
    //            Vector3Int snappedPoint = Vector3Int.zero;
    //            Vector3Int BlueprintSnappedPoint = Vector3Int.zero;
    //            if (GridOrigin)
    //            {
    //                if (hit.transform.tag == "Ground")
    //                {
    //                    snappedPoint = Vector3Int.FloorToInt((GridOrigin.transform.InverseTransformPoint(hit.point) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset);
    //                    BlueprintSnappedPoint = snappedPoint;
    //                }
    //                if (hit.transform.tag == "Tile")
    //                {
    //                    snappedPoint = Vector3Int.FloorToInt(((GridOrigin.transform.InverseTransformPoint(hit.transform.position) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset) + hit.transform.InverseTransformDirection(hit.normal));
    //                    BlueprintSnappedPoint = Vector3Int.FloorToInt((GridOrigin.transform.InverseTransformPoint(hit.transform.position) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset);
    //                }

    //                tileTest.transform.position = GridOrigin.transform.TransformPoint(new Vector3(BlueprintSnappedPoint.x * SnapOffset, BlueprintSnappedPoint.y * SnapOffset, BlueprintSnappedPoint.z * SnapOffset));
    //                tileTest.transform.eulerAngles = new Vector3(0, GridOrigin.transform.eulerAngles.y, 0);
    //            }
    //            else
    //            {
    //                tileTest.transform.position = hit.point;
    //                tileTest.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    //            }

    //            //Left Click
    //            if (Input.GetMouseButtonDown(0))
    //            {
    //                if (HouseFloors.Count == 0)
    //                {
    //                    GridOrigin = new GameObject("HouseOrigin");
    //                    GridOrigin.transform.position = hit.point;
    //                    GridOrigin.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
    //                }
    //                UpdateGridTile(true, snappedPoint * 2, PlacementType.Normal);
    //            }

    //            //Right Click
    //            if (Input.GetMouseButtonDown(1))
    //            {
    //                if (hit.transform.tag == "Tile")
    //                {
    //                    PlacementCamera.DisableRotation = true;
    //                    UpdateGridTile(false, BlueprintSnappedPoint * 2, PlacementType.Normal);
    //                }
    //            }

    //            if (Input.GetMouseButtonUp(1))
    //            {
    //                PlacementCamera.DisableRotation = false;
    //            }

    //            if (Input.GetKeyDown(KeyCode.J))
    //            {
    //                PreFinalizeBuild();
    //            }

    //            if (Input.GetKeyDown(KeyCode.K))
    //            {
    //                FinalizeBuild();
    //            }
    //        }
    //    }

    //    void UpdateGridTile(bool add, Vector3Int tileCoord, PlacementType placementType)
    //    {
    //        if (add)
    //        {
    //            //New floor
    //            if (!HouseFloors.ContainsKey(tileCoord.y))
    //            {
    //                if (HouseFloors.ContainsKey(tileCoord.y - 2)) HouseFloors.Add(tileCoord.y, new HouseFloor(HouseFloors[tileCoord.y - 2].StyleType));
    //                else HouseFloors.Add(tileCoord.y, new HouseFloor(OuterStyleType));
    //            }

    //            if (!HouseFloors[tileCoord.y].GridTiles.ContainsKey(tileCoord))
    //            {
    //                //Create tile
    //                GridTile gridTile = new GridTile();
    //                if (placementType == PlacementType.Normal)
    //                {
    //                    GameObject outerTile = Instantiate(tilePrefabTest, Vector3.zero, Quaternion.identity, GridOrigin.transform);
    //                    outerTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
    //                    outerTile.transform.localRotation = Quaternion.identity;
    //                    outerTile.transform.localScale = Vector3.one;
    //                    gridTile.Tile = outerTile;
    //                }
    //                gridTile.IsFloor = placementType == PlacementType.Normal ? false : true;
    //                //gridTile.IsFloor = false;

    //                HouseFloors[tileCoord.y].GridTiles.Add(tileCoord, gridTile);

    //                //Create new floor
    //                if (!HouseFloors.ContainsKey(tileCoord.y + 2))
    //                {
    //                    if (HouseFloors.ContainsKey(tileCoord.y)) HouseFloors.Add(tileCoord.y + 2, new HouseFloor(HouseFloors[tileCoord.y].StyleType));
    //                    else HouseFloors.Add(tileCoord.y + 2, new HouseFloor(OuterStyleType));
    //                }

    //                //Checks 1 grid tile above the current (Cieling)
    //                if (!HouseFloors[tileCoord.y + 2].GridTiles.ContainsKey(tileCoord + new Vector3Int(0, 2, 0)) && placementType == PlacementType.Normal)
    //                {
    //                    UpdateGridTile(true, tileCoord + new Vector3Int(0, 2, 0), PlacementType.Ceiling);
    //                }
    //            }
    //            else
    //            {
    //                if (HouseFloors[tileCoord.y].GridTiles[tileCoord].IsFloor)
    //                {
    //                    GridTile GridTile = new GridTile();
    //                    GameObject outerTile = Instantiate(tilePrefabTest, Vector3.zero, Quaternion.identity, GridOrigin.transform);
    //                    outerTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
    //                    outerTile.transform.localRotation = Quaternion.identity;
    //                    outerTile.transform.localScale = Vector3.one;
    //                    GridTile.Tile = outerTile;
    //                    GridTile.IsFloor = false;
    //                    HouseFloors[tileCoord.y].GridTiles[tileCoord] = GridTile;

    //                    //Create new floor
    //                    if (!HouseFloors.ContainsKey(tileCoord.y + 2))
    //                    {
    //                        if (HouseFloors.ContainsKey(tileCoord.y)) HouseFloors.Add(tileCoord.y + 2, new HouseFloor(HouseFloors[tileCoord.y].StyleType));
    //                        else HouseFloors.Add(tileCoord.y + 2, new HouseFloor(OuterStyleType));
    //                    }

    //                    //Checks 1 grid tile above the current
    //                    if (!HouseFloors[tileCoord.y + 2].GridTiles.ContainsKey(tileCoord + new Vector3Int(0, 2, 0)))
    //                    {
    //                        GridTile floorGridTile = new GridTile();
    //                        floorGridTile.IsFloor = true;

    //                        HouseFloors[tileCoord.y + 2].GridTiles.Add(tileCoord + new Vector3Int(0, 2, 0), floorGridTile);

    //                        //Update sub tiles
    //                        UpdateGridSubTile(add, tileCoord.y + 2, tileCoord + new Vector3Int(0, 2, 0) + new Vector3Int(1, 0, 1), new int[] { 3 });
    //                        UpdateGridSubTile(add, tileCoord.y + 2, tileCoord + new Vector3Int(0, 2, 0) + new Vector3Int(1, 0, -1), new int[] { 0 });
    //                        UpdateGridSubTile(add, tileCoord.y + 2, tileCoord + new Vector3Int(0, 2, 0) + new Vector3Int(-1, 0, -1), new int[] { 1 });
    //                        UpdateGridSubTile(add, tileCoord.y + 2, tileCoord + new Vector3Int(0, 2, 0) + new Vector3Int(-1, 0, 1), new int[] { 2 });
    //                    }
    //                }
    //                else Debug.Log("Warning! Attempted to add a grid that that already existed");
    //            }
    //        }
    //        else
    //        {
    //            if (HouseFloors[tileCoord.y].GridTiles[tileCoord].Tile) Destroy(HouseFloors[tileCoord.y].GridTiles[tileCoord].Tile);
    //            HouseFloors[tileCoord.y].GridTiles.Remove(tileCoord);
    //        }

    //        //Update sub tiles
    //        UpdateGridSubTile(add, tileCoord.y, tileCoord + new Vector3Int(1, 0, 1), placementType == PlacementType.Normal ? new int[] { 3, 7 } : placementType == PlacementType.Floor ? new int[] { 3 } : new int[] { 3 });
    //        UpdateGridSubTile(add, tileCoord.y, tileCoord + new Vector3Int(1, 0, -1), placementType == PlacementType.Normal ? new int[] { 0, 4 } : placementType == PlacementType.Floor ? new int[] { 0 } : new int[] { 0 });
    //        UpdateGridSubTile(add, tileCoord.y, tileCoord + new Vector3Int(-1, 0, -1), placementType == PlacementType.Normal ? new int[] { 1, 5 } : placementType == PlacementType.Floor ? new int[] { 1 } : new int[] { 1 });
    //        UpdateGridSubTile(add, tileCoord.y, tileCoord + new Vector3Int(-1, 0, 1), placementType == PlacementType.Normal ? new int[] { 2, 6 } : placementType == PlacementType.Floor ? new int[] { 2 } : new int[] { 2 });

    //        //Update adjacent tiles
    //        if (placementType == PlacementType.Normal)
    //        {
    //            UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(-2, 0, 0));
    //            UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(2, 0, 0));
    //            UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, 0, 2));
    //            UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, 0, -2));
    //            UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, 2, 0));
    //            UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, -2, 0));
    //        }

    //        if (HouseFloors[tileCoord.y].GridTiles.Count == 0 && HouseFloors[tileCoord.y].GridSubTiles.Count == 0) HouseFloors.Remove(tileCoord.y);
    //        if (HouseFloors.Count == 0) Destroy(GridOrigin);
    //    }

    //    void UpdateGridSubTile(bool add, int houseFloorIndex, Vector3Int tileCoord, int[] affectedTileDataIndexs)
    //    {
    //        //Creates a new sub tile if it doesn't already exist
    //        if (add && !HouseFloors[houseFloorIndex].GridSubTiles.ContainsKey(tileCoord)) HouseFloors[houseFloorIndex].GridSubTiles.Add(tileCoord, new GridSubTile("00000000"));

    //        //Edits tile data
    //        if (affectedTileDataIndexs.Length != 0)
    //        {
    //            char[] tileData = HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData.ToCharArray();
    //            for (int i = 0; i < affectedTileDataIndexs.Length; i++) tileData[affectedTileDataIndexs[i]] = add ? '1' : '0';
    //            HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData = new string(tileData);
    //        }

    //        //Destroy old tile if it exists
    //        for (int i = 0; i < HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].Tiles.Count; i++)
    //        {
    //            if (HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].Tiles[i]) Destroy(HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].Tiles[i]);
    //        }
    //        HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].Tiles.Clear();

    //        //If the sub tile is no longer used, destroy it's tile and remove from the dictionary
    //        if (HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData == "00000000") HouseFloors[houseFloorIndex].GridSubTiles.Remove(tileCoord);
    //        else
    //        {
    //            //Spawns/updates new sub tile into the game
    //            if (GridPatterns.ContainsKey(HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData))
    //            {
    //                for (int i = 0; i < GridPatterns[HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData].TileTypes.Length; i++)
    //                {
    //                    GameObject outerSubTile = Instantiate(Run.instance.OuterTiles[new OuterHouseTilesIndex(0, GridPatterns[HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData].TileTypes[i])].TileVariants[0], Vector3.zero, Quaternion.identity, GridOrigin.transform);
    //                    outerSubTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
    //                    outerSubTile.transform.localEulerAngles = new Vector3(0, i == 0 ? GridPatterns[HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData].RotationAngle : GridPatterns[HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData.Substring(4, 4) + HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData.Substring(4, 4)].RotationAngle, 0);
    //                    //outerSubTile.transform.localEulerAngles = GridPatterns[HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData].Rotate ? new Vector3(0, 90, 0) : Vector3.zero;
    //                    //outerSubTile.transform.localScale = GridPatterns[HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData].Invert ? new Vector3(-1, 1, 1) : Vector3.one;

    //                    HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].Tiles.Add(outerSubTile);
    //                }
    //            }
    //            else Debug.Log("Warning! Tile data: " + HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData + " is not registered");
    //        }
    //    }

    //    void UpdateAdjacentTiles(bool add, Vector3Int tileCoord, Vector3Int targetTileCoord)
    //    {
    //        //If the floor number exists
    //        if (HouseFloors.ContainsKey(targetTileCoord.y))
    //        {
    //            //If the tile exists
    //            if (HouseFloors[targetTileCoord.y].GridTiles.ContainsKey(targetTileCoord))
    //            {
    //                //Update adjacent to both the current grid tile and the targeted grid tile
    //                if (add)
    //                {
    //                    HouseFloors[tileCoord.y].GridTiles[tileCoord].AdjacentTiles.Add(targetTileCoord);
    //                    HouseFloors[targetTileCoord.y].GridTiles[targetTileCoord].AdjacentTiles.Add(tileCoord);
    //                }
    //                else HouseFloors[targetTileCoord.y].GridTiles[targetTileCoord].AdjacentTiles.Remove(tileCoord);
    //            }
    //        }
    //    }

    //    void CheckForFloatingTiles()
    //    {
    //        //Checks if ground floor exists
    //        if (HouseFloors.ContainsKey(0))
    //        {
    //            //Search for tiles connected to the ground
    //            foreach (Vector3Int gridTileCoord in HouseFloors[0].GridTiles.Keys)
    //            {
    //                //If tile hasn't been visited
    //                if (!VisitedTiles.Contains(gridTileCoord)) CheckAdjacentTiles(gridTileCoord, ref VisitedTiles);
    //            }

    //            //Search for tiles unconnected to the ground
    //            foreach (HouseFloor floor in HouseFloors.Values)
    //            {
    //                foreach (Vector3Int gridTileCoord in floor.GridTiles.Keys)
    //                {
    //                    if (!VisitedTiles.Contains(gridTileCoord)) UngroundedTiles.Add(gridTileCoord);
    //                }
    //            }
    //        }
    //    }

    //    void DestroyFloatingTiles()
    //    {
    //        //Destroy unconnected tiles
    //        foreach (Vector3Int gridTileCoord in UngroundedTiles) UpdateGridTile(false, gridTileCoord, PlacementType.Normal);
    //    }

    //    void CheckAdjacentTiles(Vector3Int tileCoord, ref HashSet<Vector3Int> visitedTiles)
    //    {
    //        visitedTiles.Add(tileCoord);
    //        foreach (Vector3Int adjacentTileCoord in HouseFloors[tileCoord.y].GridTiles[tileCoord].AdjacentTiles)
    //        {
    //            if (!visitedTiles.Contains(adjacentTileCoord)) CheckAdjacentTiles(adjacentTileCoord, ref visitedTiles);
    //        }
    //    }

    //    void CreateCentroidPivot()
    //    {
    //        if (GridOrigin)
    //        {
    //            float xSum = 0;
    //            float zSum = 0;
    //            int count = 0;
    //            HashSet<Vector3Int> uniqueFlattenCoords = new HashSet<Vector3Int>();
    //            foreach (HouseFloor floor in HouseFloors.Values)
    //            {
    //                foreach (Vector3Int gridTileCoord in floor.GridTiles.Keys)
    //                {
    //                    Vector3Int flattenCoord = new Vector3Int(gridTileCoord.x, 0, gridTileCoord.z);
    //                    if (!uniqueFlattenCoords.Contains(flattenCoord))
    //                    {
    //                        xSum += (gridTileCoord.x * 0.5f) * SnapOffset;
    //                        zSum += (gridTileCoord.z * 0.5f) * SnapOffset;
    //                        count += 1;
    //                        uniqueFlattenCoords.Add(flattenCoord);
    //                    }
    //                }
    //            }

    //            if (count != 0)
    //            {
    //                GameObject centroidPivot = new GameObject("House");
    //                centroidPivot.transform.position = GridOrigin.transform.TransformPoint(new Vector3(xSum / count, 0, zSum / count));
    //                centroidPivot.transform.rotation = GridOrigin.transform.rotation;
    //                GridOrigin.transform.SetParent(centroidPivot.transform);
    //                centroidPivot.AddComponent<BoxCollider>();
    //                CalculateBounds(centroidPivot);
    //            }
    //        }
    //    }

    //    void CalculateBounds(GameObject parent)
    //    {
    //        BoxCollider collider = parent.GetComponent<BoxCollider>();
    //        if (collider)
    //        {
    //            Bounds bounds = new Bounds(parent.transform.position, Vector3.zero);

    //            //Rotates house back to default rotation so bounds can be encapsulated accurately
    //            Vector3 previousEulerAngles = parent.transform.eulerAngles;
    //            parent.transform.eulerAngles = Vector3.zero;

    //            //Todo: use sub grid instead
    //            foreach (HouseFloor floor in HouseFloors.Values)
    //            {
    //                foreach (GridTile gridTileCoord in floor.GridTiles.Values) bounds.Encapsulate(gridTileCoord.Tile.GetComponent<Renderer>().bounds);
    //            }
    //            collider.center = parent.transform.InverseTransformPoint(bounds.center);
    //            collider.size = bounds.size;

    //            //Rotates house back to it's assigned rotation prior to calculations since Encapsulated has been accurately calculated
    //            parent.transform.eulerAngles = previousEulerAngles;
    //        }
    //        else Debug.Log("Warning! Missing BoxCollider for " + parent.name + ". Could not Calculate bounds");
    //    }

    //    public void CreateNewHouse()
    //    {

    //    }

    //    void SelectHouseToEdit()
    //    {

    //    }

    //    //Run before calling FinalizeBuild()
    //    public void PreFinalizeBuild()
    //    {
    //        VisitedTiles.Clear();
    //        UngroundedTiles.Clear();

    //        CheckForFloatingTiles();
    //        if (UngroundedTiles.Count > 0)
    //        {
    //            //Invoke event to enable UI
    //        }
    //    }

    //    //Run after calling PreFinalizeBuild();
    //    public void FinalizeBuild()
    //    {
    //        DestroyFloatingTiles();
    //        if (HouseFloors.ContainsKey(0)) CreateCentroidPivot();
    //    }
    //}
}
