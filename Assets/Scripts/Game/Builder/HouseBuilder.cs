using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//House Builder
public class HouseBuilder : MonoBehaviour
{
    public LayerMask SelectionLayer;
    public LayerMask SelectionBoxLayer;
    //Stores all unique patterns
    public List<GridPattern> GridPatternTemplates = new List<GridPattern>();
    public int OuterStyleType = 0;
    public float SnapOffset = 0.68f;
    public BuilderSelectionBox SelectionBox;
    public GameObject GridTilePrefab;
    public Material SelectionBoxMouseOverMaterial;
    public Material SelectionBoxNormalMaterial;

    //Stores all tiles used the house
    private Dictionary<Vector3Int, GridTile> GridTiles = new Dictionary<Vector3Int, GridTile>();
    private Dictionary<Vector3Int, GridSubTile> GridSubTiles = new Dictionary<Vector3Int, GridSubTile>();
    //Stores all possible varients of each pattern
    private Dictionary<string, GridPatternVarient> GridPatterns = new Dictionary<string, GridPatternVarient>();

    //Grid origin and parent object for the house
    private GameObject GridOrigin;

    //Floating tiles check
    private HashSet<Vector3Int> VisitedTiles = new HashSet<Vector3Int>();
    private HashSet<Vector3Int> UngroundedTiles = new HashSet<Vector3Int>();

    private GameObject CurrentHoveredSelectionFace;

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
            GridPatterns.Add(GridPatternTemplates[i].PatternData, new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, false, 0));

            if (GridPatternTemplates[i].PatternData == "00000000" || GridPatternTemplates[i].PatternData == "11111111") continue;

            //Rotates the pattern by 90 degrees each iteration (90 - 270)
            char[] previousRotatedPatternData = GridPatternTemplates[i].PatternData.ToCharArray();
            float previousRotationAngle = 0;
            for (int j = 0; j < 3; j++)
            {
                char[] currentRotatedPatternData = new char[previousRotatedPatternData.Length];
                //Bottom 2x2
                currentRotatedPatternData[0] = previousRotatedPatternData[3];
                currentRotatedPatternData[1] = previousRotatedPatternData[0];
                currentRotatedPatternData[2] = previousRotatedPatternData[1];
                currentRotatedPatternData[3] = previousRotatedPatternData[2];
                //Top 2x2
                currentRotatedPatternData[4] = previousRotatedPatternData[7];
                currentRotatedPatternData[5] = previousRotatedPatternData[4];
                currentRotatedPatternData[6] = previousRotatedPatternData[5];
                currentRotatedPatternData[7] = previousRotatedPatternData[6];

                previousRotationAngle += 90;

                if (!GridPatterns.ContainsKey(new string(currentRotatedPatternData)))
                {
                    if (j == 0) GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, true, previousRotationAngle));
                    else if (j == 1) GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, false, true, previousRotationAngle));
                    else GridPatterns.Add(new string(currentRotatedPatternData), new GridPatternVarient(GridPatternTemplates[i].TileTypes, true, false, previousRotationAngle));
                }

                previousRotatedPatternData = currentRotatedPatternData;
            }
        }
    }

    void Hover()
    {
        //Hovers over tiles and the ground for selection 
        RaycastHit selectionHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out selectionHit, Mathf.Infinity, SelectionLayer))
        {
            //Blueprint placement
            Vector3Int snappedPoint = Vector3Int.zero;
            Vector3Int BlueprintSnappedPoint = Vector3Int.zero;
            Vector2 selectionBoxDimension = new Vector2(SnapOffset, 0.1f);
            if (GridOrigin)
            {
                if (selectionHit.transform.tag == "Ground")
                {
                    snappedPoint = Vector3Int.FloorToInt((GridOrigin.transform.InverseTransformPoint(selectionHit.point) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset);
                    BlueprintSnappedPoint = snappedPoint;
                }
                if (selectionHit.transform.tag == "Tile")
                {
                    selectionBoxDimension = new Vector2(SnapOffset, SnapOffset);
                    snappedPoint = Vector3Int.FloorToInt(((GridOrigin.transform.InverseTransformPoint(selectionHit.transform.position) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset) + selectionHit.transform.InverseTransformDirection(selectionHit.normal));
                    BlueprintSnappedPoint = Vector3Int.FloorToInt((GridOrigin.transform.InverseTransformPoint(selectionHit.transform.position) + new Vector3(SnapOffset * 0.5f, SnapOffset * 0.5f, SnapOffset * 0.5f)) / SnapOffset);
                }

                SelectionBox.transform.position = GridOrigin.transform.TransformPoint(new Vector3(BlueprintSnappedPoint.x * SnapOffset, BlueprintSnappedPoint.y * SnapOffset, BlueprintSnappedPoint.z * SnapOffset));
                SelectionBox.transform.eulerAngles = new Vector3(0, GridOrigin.transform.eulerAngles.y, 0);
            }
            else
            {
                SelectionBox.transform.position = selectionHit.point;
                SelectionBox.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
            }

            //Selectionbox dimension change
            if (SelectionBox.BoxDimension != selectionBoxDimension) SelectionBox.BoxDimension = selectionBoxDimension;

            //Selectionbox mouse over
            RaycastHit selectionBoxHit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out selectionBoxHit, Mathf.Infinity, SelectionBoxLayer))
            {
                if (selectionBoxHit.collider.gameObject != CurrentHoveredSelectionFace)
                {
                    if (CurrentHoveredSelectionFace) CurrentHoveredSelectionFace.GetComponent<MeshRenderer>().material = SelectionBoxNormalMaterial;
                    selectionBoxHit.collider.GetComponent<MeshRenderer>().material = SelectionBoxMouseOverMaterial;
                    CurrentHoveredSelectionFace = selectionBoxHit.collider.gameObject;
                }
            }

            //Left Click
            if (Input.GetMouseButtonDown(0))
            {
                if (GridTiles.Count == 0)
                {
                    GridOrigin = new GameObject("HouseOrigin");
                    GridOrigin.transform.position = selectionHit.point;
                    GridOrigin.transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);
                }
                UpdateGridTile(true, snappedPoint * 2);
            }

            //Right Click
            if (Input.GetMouseButtonDown(1))
            {
                if (selectionHit.transform.tag == "Tile")
                {
                    PlacementCamera.DisableRotation = true;
                    UpdateGridTile(false, BlueprintSnappedPoint * 2);
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
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

    void UpdateGridTile(bool add, Vector3Int tileCoord)
    {
        if (add)
        {
            //Creates a new grid tile if it doesn't exist already and spawns a new grid tile into the game
            if (!GridTiles.ContainsKey(tileCoord))
            {
                GameObject outerTile = Instantiate(GridTilePrefab, Vector3.zero, Quaternion.identity, GridOrigin.transform);
                outerTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
                outerTile.transform.localRotation = Quaternion.identity;

                GridTile gridTile = new GridTile
                {
                    Tile = outerTile
                };

                GridTiles.Add(tileCoord, gridTile);
            }
        }
        else
        {
            //Destroys grid tile
            if (GridTiles[tileCoord].Tile) Destroy(GridTiles[tileCoord].Tile);
            else Debug.Log("Warning! Attempted to destroy a tile that does not exist @UpdateGridTile()");
            GridTiles.Remove(tileCoord);
        }

        //Update grid sub tiles
        //Top 2x2
        UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, 1, 1), new int[] { 2 });
        UpdateGridSubTile(add, tileCoord + new Vector3Int(1, 1, 1), new int[] { 3 });
        UpdateGridSubTile(add, tileCoord + new Vector3Int(1, 1, -1), new int[] { 0 });
        UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, 1, -1), new int[] { 1 });
        //Bottom 2x2
        UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, -1, 1), new int[] { 6 });
        UpdateGridSubTile(add, tileCoord + new Vector3Int(1, -1, 1), new int[] { 7 });
        UpdateGridSubTile(add, tileCoord + new Vector3Int(1, -1, -1), new int[] { 4 });
        UpdateGridSubTile(add, tileCoord + new Vector3Int(-1, -1, -1), new int[] { 5 });

        //Update adjacent grid tiles
        UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(-2, 0, 0));
        UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(2, 0, 0));
        UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, 0, 2));
        UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, 0, -2));
        UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, 2, 0));
        UpdateAdjacentTiles(add, tileCoord, tileCoord + new Vector3Int(0, -2, 0));

        if (GridTiles.Count == 0) Destroy(GridOrigin);
    }

    void UpdateGridSubTile(bool add, Vector3Int tileCoord, int[] affectedTileDataIndexs)
    {
        //Creates a new grid sub tile if it doesn't exist
        if (add && !GridSubTiles.ContainsKey(tileCoord)) GridSubTiles.Add(tileCoord, new GridSubTile("00000000"));

        //Edits tile data
        if (affectedTileDataIndexs.Length != 0)
        {
            char[] tileData = GridSubTiles[tileCoord].TileData.ToCharArray();
            for (int i = 0; i < affectedTileDataIndexs.Length; i++) tileData[affectedTileDataIndexs[i]] = add ? '1' : '0';
            GridSubTiles[tileCoord].TileData = new string(tileData);
        }

        //Destroy old sub tiles if it exists
        for (int i = 0; i < GridSubTiles[tileCoord].Tiles.Count; i++)
        {
            if (GridSubTiles[tileCoord].Tiles[i]) Destroy(GridSubTiles[tileCoord].Tiles[i]);
            else Debug.Log("Warning! Attempted to destroy a sub tile that does not exist @UpdateGridSubTile()");
        }
        GridSubTiles[tileCoord].Tiles.Clear();

        //If the grid sub tile is no longer used, destroy it's tile and remove from the dictionary
        if (GridSubTiles[tileCoord].TileData == "00000000") GridSubTiles.Remove(tileCoord);
        else
        {
            //Spawns/updates new sub tiles into the game
            if (GridPatterns.ContainsKey(GridSubTiles[tileCoord].TileData))
            {
                for (int i = 0; i < GridPatterns[GridSubTiles[tileCoord].TileData].TileTypes.Length; i++)
                {
                    GameObject outerSubTile = Instantiate(Run.instance.OuterTiles[new OuterHouseTilesIndex(0, GridPatterns[GridSubTiles[tileCoord].TileData].TileTypes[i])].TileVariants[0], Vector3.zero, Quaternion.identity, GridOrigin.transform);
                    outerSubTile.transform.localPosition = new Vector3((tileCoord.x * 0.5f) * SnapOffset, (tileCoord.y * 0.5f) * SnapOffset, (tileCoord.z * 0.5f) * SnapOffset);
                    outerSubTile.transform.localEulerAngles = new Vector3(0, i == 0 ? GridPatterns[GridSubTiles[tileCoord].TileData].RotationAngle : GridPatterns[GridSubTiles[tileCoord].TileData.Substring(4, 4) + GridSubTiles[tileCoord].TileData.Substring(4, 4)].RotationAngle, 0);

                    GridSubTiles[tileCoord].Tiles.Add(outerSubTile);
                }
            }
            else Debug.Log("Warning! Tile data: " + GridSubTiles[tileCoord].TileData + " is not registered @UpdateGridSubTile()");
        }
    }

    void UpdateAdjacentTiles(bool add, Vector3Int tileCoord, Vector3Int targetTileCoord)
    {
        //If the tile exists
        if (GridTiles.ContainsKey(targetTileCoord))
        {
            //Update adjacent to both the current grid tile and the targeted grid tile
            if (add)
            {
                GridTiles[tileCoord].AdjacentTiles.Add(targetTileCoord);
                GridTiles[targetTileCoord].AdjacentTiles.Add(tileCoord);
            }
            else GridTiles[targetTileCoord].AdjacentTiles.Remove(tileCoord);
        }
    }

    void CheckForFloatingTiles()
    {
        //Search for grid tiles connected to the ground
        foreach (Vector3Int gridTileCoord in GridTiles.Keys)
        {
            if (gridTileCoord.y == 0)
            {
                //If tile hasn't been visited
                if (!VisitedTiles.Contains(gridTileCoord)) CheckAdjacentTiles(gridTileCoord, ref VisitedTiles);
            }
        }

        //Search for grid tiles unconnected to the ground
        foreach (Vector3Int gridTileCoord in GridTiles.Keys)
        {
            if (!VisitedTiles.Contains(gridTileCoord)) UngroundedTiles.Add(gridTileCoord);
        }
    }

    void DestroyFloatingTiles()
    {
        //Destroy unconnected grid tiles
        foreach (Vector3Int gridTileCoord in UngroundedTiles) UpdateGridTile(false, gridTileCoord);
    }

    void CheckAdjacentTiles(Vector3Int tileCoord, ref HashSet<Vector3Int> visitedTiles)
    {
        visitedTiles.Add(tileCoord);
        foreach (Vector3Int adjacentTileCoord in GridTiles[tileCoord].AdjacentTiles)
        {
            if (!visitedTiles.Contains(adjacentTileCoord)) CheckAdjacentTiles(adjacentTileCoord, ref visitedTiles);
        }
    }

    void CreateCentroidPivot()
    {
        if (GridOrigin)
        {
            float xSum = 0;
            float zSum = 0;
            int count = 0;
            HashSet<Vector3Int> uniqueFlattenCoords = new HashSet<Vector3Int>();
            foreach (Vector3Int gridTileCoord in GridTiles.Keys)
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

            if (count != 0)
            {
                GameObject centroidPivot = new GameObject("House");
                centroidPivot.transform.position = GridOrigin.transform.TransformPoint(new Vector3(xSum / count, 0, zSum / count));
                centroidPivot.transform.rotation = GridOrigin.transform.rotation;
                GridOrigin.transform.SetParent(centroidPivot.transform);
                centroidPivot.AddComponent<BoxCollider>();
                CalculateBounds(centroidPivot);
            }
            else Debug.Log("Warning! Empty grid, could not create centroid pivot @CreateCentroidPivot()");
        }
        else Debug.Log("Warning! Origin does not exist @CreateCentroidPivot()");
    }

    void CalculateBounds(GameObject parent)
    {
        BoxCollider collider = parent.GetComponent<BoxCollider>();
        if (collider)
        {
            Bounds bounds = new Bounds(parent.transform.position, Vector3.zero);

            //Rotates back to default rotation so bounds can be encapsulated accurately
            Vector3 previousEulerAngles = parent.transform.eulerAngles;
            parent.transform.eulerAngles = Vector3.zero;

            foreach (GridSubTile subGridTileCoord in GridSubTiles.Values)
            {
                foreach (GameObject subTile in subGridTileCoord.Tiles) bounds.Encapsulate(subTile.GetComponent<Renderer>().bounds);
            }
            collider.center = parent.transform.InverseTransformPoint(bounds.center);
            collider.size = bounds.size;

            //Rotates back to it's assigned rotation prior to calculations since encapsulation has been accurately calculated
            parent.transform.eulerAngles = previousEulerAngles;
        }
        else Debug.Log("Warning! Missing BoxCollider for " + parent.name + ", could not calculate bounds @CalculateBounds()");
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
        if (UngroundedTiles.Count > 0)
        {
            //Invoke event to enable UI
        }
    }

    //Run after calling PreFinalizeBuild();
    public void FinalizeBuild()
    {
        DestroyFloatingTiles();
        CreateCentroidPivot();
    }
}
