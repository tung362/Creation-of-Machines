using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseBuilder : MonoBehaviour
{
    public LayerMask Selection;
    //Stores all unique patterns
    public List<GridPattern> GridPatternTemplates = new List<GridPattern>();
    public int OuterStyleType = 0;
    public float SnapOffset = 0.68f;
    public GameObject tileTest;
    public GameObject tilePrefabTest;

    //Stores all tiles used by each floor of the house
    private Dictionary<int, HouseFloor> HouseFloors = new Dictionary<int, HouseFloor>();
    //Stores all possible varients of each pattern
    private Dictionary<string, GridPatternVarient> GridPatterns = new Dictionary<string, GridPatternVarient>();

    private HouseFloor CurrentFloor;

    //Grid origin and parent object for the house
    private GameObject GridOrigin;

    private HashSet<Vector3Int> VisitedTiles = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> UngroundedTiles = new HashSet<Vector3Int>();

    void Start()
    {
        CreateAllPatternVarients();
    }

    void Update()
    {
        Hover();
    }

    //Creates all pattern varients from the pattern template for a 2x2 box and add it to the dictionary
    void CreateAllPatternVarients()
    {
        for (int i = 0; i < GridPatternTemplates.Count; i++)
        {
            if (GridPatternTemplates[i].PatternData == "00000000" || GridPatternTemplates[i].PatternData == "11111111") continue;
            GridPatterns.Add(GridPatternTemplates[i].PatternData, new GridPatternVarient(GridPatternTemplates[i].TileType, false, false));

            //Rotates the pattern by 90 degrees each iteration (90 - 270)
            char[] previousRotatedPatternData = GridPatternTemplates[i].PatternData.ToCharArray();
            for (int j = 0; j < 3; j++)
            {
                char[] CurrentRotatedPatternData = new char[previousRotatedPatternData.Length];
                //Bottom 2x1
                CurrentRotatedPatternData[0] = previousRotatedPatternData[3];
                CurrentRotatedPatternData[1] = previousRotatedPatternData[0];
                CurrentRotatedPatternData[2] = previousRotatedPatternData[1];
                CurrentRotatedPatternData[3] = previousRotatedPatternData[2];
                //Top 2x1
                CurrentRotatedPatternData[4] = previousRotatedPatternData[7];
                CurrentRotatedPatternData[5] = previousRotatedPatternData[4];
                CurrentRotatedPatternData[6] = previousRotatedPatternData[5];
                CurrentRotatedPatternData[7] = previousRotatedPatternData[6];

                if (j == 0) GridPatterns.Add(new string(CurrentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileType, true, true));
                else if (j == 1) GridPatterns.Add(new string(CurrentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileType, false, true));
                else GridPatterns.Add(new string(CurrentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileType, true, false));

                previousRotatedPatternData = CurrentRotatedPatternData;
            }
        }
    }

    void Hover()
    {
        //Hovers over tiles and the ground for selection 
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, Selection))
        {
            //Blueprint placement
            Vector3Int snappedPoint = Vector3Int.zero;
            Vector3Int BlueprintSnappedPoint = Vector3Int.zero;
            if (GridOrigin)
            {
                if (hit.transform.tag == "Ground")
                {
                    snappedPoint = Vector3Int.FloorToInt((GridOrigin.transform.InverseTransformPoint(hit.point) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset);
                    BlueprintSnappedPoint = snappedPoint;
                }
                if (hit.transform.tag == "Tile")
                {
                    snappedPoint = Vector3Int.FloorToInt(((GridOrigin.transform.InverseTransformPoint(hit.transform.position) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset) + hit.transform.InverseTransformDirection(hit.normal));
                    BlueprintSnappedPoint = Vector3Int.FloorToInt((GridOrigin.transform.InverseTransformPoint(hit.transform.position) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset);
                }

                tileTest.transform.position = GridOrigin.transform.TransformPoint(new Vector3(BlueprintSnappedPoint.x * SnapOffset, BlueprintSnappedPoint.y * SnapOffset, BlueprintSnappedPoint.z * SnapOffset));
                tileTest.transform.eulerAngles = new Vector3(0, GridOrigin.transform.eulerAngles.y, 0);
            }
            else
            {
                tileTest.transform.position = hit.point;
                tileTest.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
            }

            //Left Click
            if (Input.GetMouseButtonDown(0))
            {
                if (HouseFloors.Count == 0)
                {
                    GridOrigin = new GameObject("HouseOrigin");
                    GridOrigin.transform.position = hit.point;
                    GridOrigin.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
                }
                CreateGridTile(snappedPoint * 2);
            }

            //Right Click
            if (Input.GetMouseButtonDown(1))
            {
                if (hit.transform.tag == "Tile")
                {
                    PlacementCamera.DisableRotation = true;
                    DestroyGridTile(BlueprintSnappedPoint * 2);
                }
            }

            if(Input.GetMouseButtonUp(1))
            {
                PlacementCamera.DisableRotation = false;
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                PreFinalizeBuild();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                FinalizeBuild();
            }
        }
    }

    void CreateGridTile(Vector3Int tileCoord)
    {
        //New floor
        if(!HouseFloors.ContainsKey(tileCoord.y))
        {
            if(HouseFloors.ContainsKey(tileCoord.y - 2)) HouseFloors.Add(tileCoord.y, new HouseFloor(HouseFloors[tileCoord.y - 2].StyleType));
            else HouseFloors.Add(tileCoord.y, new HouseFloor(OuterStyleType));
        }

        if (!HouseFloors[tileCoord.y].GridTiles.ContainsKey(tileCoord))
        {
            //Create tile
            GridTile gridTile = new GridTile();
            GameObject outerTile = Instantiate(tilePrefabTest, Vector3.zero, Quaternion.identity, GridOrigin.transform); //Todo: add spawning
            outerTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
            outerTile.transform.localRotation = Quaternion.identity;
            outerTile.transform.localScale = Vector3.one;
            gridTile.Tile = outerTile;

            HouseFloors[tileCoord.y].GridTiles.Add(tileCoord, gridTile);

            //Create sub tiles
            CreateGridSubTile(tileCoord.y, tileCoord + new Vector3Int(1, 0, 1), new int[] { 3, 7 });
            CreateGridSubTile(tileCoord.y, tileCoord + new Vector3Int(1, 0, -1), new int[] { 0, 4 });
            CreateGridSubTile(tileCoord.y, tileCoord + new Vector3Int(-1, 0, -1), new int[] { 1, 5 });
            CreateGridSubTile(tileCoord.y, tileCoord + new Vector3Int(-1, 0, 1), new int[] { 2, 6 });

            //Update adjacent tiles
            UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(-2, 0, 0), true);
            UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(2, 0, 0), true);
            UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, 0, 2), true);
            UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, 0, -2), true);
            UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, 2, 0), true);
            UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, -2, 0), true);
        }
    }

    void DestroyGridTile(Vector3Int tileCoord)
    {
        Destroy(HouseFloors[tileCoord.y].GridTiles[tileCoord].Tile);
        HouseFloors[tileCoord.y].GridTiles.Remove(tileCoord);

        //destroy sub tiles
        DestroyGridSubTile(tileCoord.y, tileCoord + new Vector3Int(1, 0, 1), new int[] { 3, 7 });
        DestroyGridSubTile(tileCoord.y, tileCoord + new Vector3Int(1, 0, -1), new int[] { 0, 4 });
        DestroyGridSubTile(tileCoord.y, tileCoord + new Vector3Int(-1, 0, -1), new int[] { 1, 5 });
        DestroyGridSubTile(tileCoord.y, tileCoord + new Vector3Int(-1, 0, 1), new int[] { 2, 6 });

        //Update adjacent tiles
        UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(-2, 0, 0), false);
        UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(2, 0, 0), false);
        UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, 0, 2), false);
        UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, 0, -2), false);
        UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, 2, 0), false);
        UpdateAdjacentTiles(tileCoord, tileCoord + new Vector3Int(0, -2, 0), false);

        if (HouseFloors[tileCoord.y].GridTiles.Count == 0 && HouseFloors[tileCoord.y].GridSubTiles.Count == 0) HouseFloors.Remove(tileCoord.y);
        if(HouseFloors.Count == 0) Destroy(GridOrigin);
    }

    //Creates/edit sub tiles
    void CreateGridSubTile(int houseFloorIndex, Vector3Int tileCoord, int[] affectedTileDataIndexs)
    {
        //Creates a new sub tile if it doesn't already exist
        if (!HouseFloors[houseFloorIndex].GridSubTiles.ContainsKey(tileCoord)) HouseFloors[houseFloorIndex].GridSubTiles.Add(tileCoord, new GridSubTile("00000000"));

        //Edits tile data
        if (affectedTileDataIndexs.Length != 0)
        {
            char[] tileData = HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData.ToCharArray();
            for (int i = 0; i < affectedTileDataIndexs.Length; i++) tileData[affectedTileDataIndexs[i]] = '1';
            HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData = new string(tileData);
        }

        //Spawns the sub tile into the game
        HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].Tile = null; //Todo: add spawning
    }

    void DestroyGridSubTile(int houseFloorIndex, Vector3Int tileCoord, int[] affectedTileDataIndexs)
    {
        //Edits tile data
        if (affectedTileDataIndexs.Length != 0)
        {
            char[] tileData = HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData.ToCharArray();
            for (int i = 0; i < affectedTileDataIndexs.Length; i++) tileData[affectedTileDataIndexs[i]] = '0';
            HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData = new string(tileData);
        }

        //If the sub tile is no longer used destroy it's tile and remove from the dictionary
        if(HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].TileData == "00000000")
        {
            //Destroy(HouseFloors[houseFloorIndex].GridSubTiles[tileCoord].Tile);
            HouseFloors[houseFloorIndex].GridSubTiles.Remove(tileCoord);
        }
    }

    void UpdateAdjacentTiles(Vector3Int tileCoord, Vector3Int targetTileCoord, bool Add)
    {
        //If the floor number exists
        if(HouseFloors.ContainsKey(targetTileCoord.y))
        {
            //If the tile exists
            if (HouseFloors[targetTileCoord.y].GridTiles.ContainsKey(targetTileCoord))
            {
                //Update adjacent to both the current grid tile and the targeted grid tile
                if (Add)
                {
                    HouseFloors[tileCoord.y].GridTiles[tileCoord].AdjacentTiles.Add(targetTileCoord);
                    HouseFloors[targetTileCoord.y].GridTiles[targetTileCoord].AdjacentTiles.Add(tileCoord);
                }
                else HouseFloors[targetTileCoord.y].GridTiles[targetTileCoord].AdjacentTiles.Remove(tileCoord);
            }
        }
    }

    void CheckForFloatingTiles()
    {
        //Checks if ground floor exists
        if (HouseFloors.ContainsKey(0))
        {
            //Search for tiles connected to the ground
            foreach (Vector3Int gridTileCoord in HouseFloors[0].GridTiles.Keys)
            {
                //If tile hasn't been visited
                if (!VisitedTiles.Contains(gridTileCoord)) CheckAdjacentTiles(gridTileCoord, ref VisitedTiles);
            }

            //Search for tiles unconnected to the ground
            foreach (HouseFloor floor in HouseFloors.Values)
            {
                foreach (Vector3Int gridTileCoord in floor.GridTiles.Keys)
                {
                    if (!VisitedTiles.Contains(gridTileCoord)) UngroundedTiles.Add(gridTileCoord);
                }
            }
        }
    }

    void DestroyFloatingTiles()
    {
        //Destroy unconnected tiles
        foreach (Vector3Int gridTileCoord in UngroundedTiles) DestroyGridTile(gridTileCoord);
    }

    void CheckAdjacentTiles(Vector3Int tileCoord, ref HashSet<Vector3Int> visitedTiles)
    {
        visitedTiles.Add(tileCoord);
        foreach(Vector3Int adjacentTileCoord in HouseFloors[tileCoord.y].GridTiles[tileCoord].AdjacentTiles)
        {
            if (!visitedTiles.Contains(adjacentTileCoord)) CheckAdjacentTiles(adjacentTileCoord, ref visitedTiles);
        }
    }

    void CreateCentroidPivot()
    {
        if(GridOrigin)
        {
            float xSum = 0;
            float zSum = 0;
            int count = 0;
            HashSet<Vector3Int> uniqueFlattenCoords = new HashSet<Vector3Int>();
            foreach (HouseFloor floor in HouseFloors.Values)
            {
                foreach (Vector3Int gridTileCoord in floor.GridTiles.Keys)
                {
                    Vector3Int flattenCoord = new Vector3Int(gridTileCoord.x, 0, gridTileCoord.z);
                    if (!uniqueFlattenCoords.Contains(flattenCoord))
                    {
                        xSum += (gridTileCoord.x * 0.5f) * SnapOffset;
                        zSum += (gridTileCoord.z * 0.5f) * SnapOffset;
                        count += 1;
                        uniqueFlattenCoords.Add(flattenCoord);
                    }
                }
            }

            if(count != 0)
            {
                GameObject centroidPivot = new GameObject("House");
                centroidPivot.transform.position = GridOrigin.transform.TransformPoint(new Vector3(xSum / count, 0, zSum / count));
                centroidPivot.transform.rotation = GridOrigin.transform.rotation;
                GridOrigin.transform.SetParent(centroidPivot.transform);
                centroidPivot.AddComponent<BoxCollider>();
                CalculateBounds(centroidPivot);
            }
        }
    }

    void CalculateBounds(GameObject parent)
    {
        BoxCollider collider = parent.GetComponent<BoxCollider>();
        if (collider)
        {
            Bounds bounds = new Bounds(parent.transform.position, Vector3.zero);

            //Rotates house back to default rotation so bounds can be Encapsulated accurately
            Vector3 previousEulerAngles = parent.transform.eulerAngles;
            parent.transform.eulerAngles = Vector3.zero;

            //Todo: use sub grid instead
            foreach (HouseFloor floor in HouseFloors.Values)
            {
                foreach (GridTile gridTileCoord in floor.GridTiles.Values) bounds.Encapsulate(gridTileCoord.Tile.GetComponent<Renderer>().bounds);
            }
            collider.center = parent.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size;

            //Rotates house back to it's assigned rotation prior to calculations since Encapsulated has been accurately calculated
            parent.transform.eulerAngles = previousEulerAngles;
        }
        else Debug.Log("Warning! Missing BoxCollider for " + parent.name + ". Could not Calculate bounds");
    }

    public void CreateNewHouse()
    {

    }

    void SelectHouseToEdit()
    {

    }

    //Run before calling FinalizeBuild()
    public void PreFinalizeBuild()
    {
        VisitedTiles.Clear();
        UngroundedTiles.Clear();

        CheckForFloatingTiles();
        if(UngroundedTiles.Count > 0)
        {
            //Invoke event to enable UI
        }
    }

    //Run after calling PreFinalizeBuild();
    public void FinalizeBuild()
    {
        DestroyFloatingTiles();
        if(HouseFloors.ContainsKey(0)) CreateCentroidPivot();
    }
}
