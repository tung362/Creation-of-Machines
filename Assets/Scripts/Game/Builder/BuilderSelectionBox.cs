using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Selection box dimension change applies here
public class BuilderSelectionBox : MonoBehaviour
{
    //List of childrens
    public List<GameObject> SelectorCorners;
    public List<GameObject> SelectorSideFaces;
    public List<GameObject> SelectorTopBottomFaces;
    //Should reference default selection box prefab
    public BuilderSelectionBox SelectionBoxPrefab;

    //Selection box dimensions
    private Vector2 _BoxDimension = Vector3.one;
    public Vector2 BoxDimension
    {
        get { return _BoxDimension; }

        set
        {
            _BoxDimension = value;

            for (int i = 0; i < SelectorCorners.Count; i++)
            {
                SelectorCorners[i].transform.localPosition = new Vector3(SelectionBoxPrefab.SelectorCorners[i].transform.localPosition.x * value.x, (SelectionBoxPrefab.SelectorCorners[i].transform.localPosition.y * value.y) + (SelectionBoxPrefab.SelectorCorners[i].transform.localPosition.y > 0 ? 0.08f : -0.08f), SelectionBoxPrefab.SelectorCorners[i].transform.localPosition.z * value.x);
            }

            for (int i = 0; i < SelectorSideFaces.Count; i++)
            {
                SelectorSideFaces[i].transform.localPosition = new Vector3(SelectionBoxPrefab.SelectorSideFaces[i].transform.localPosition.x * value.x, SelectionBoxPrefab.SelectorSideFaces[i].transform.localPosition.y * value.y, SelectionBoxPrefab.SelectorSideFaces[i].transform.localPosition.z * value.x);
                SelectorSideFaces[i].transform.localScale = new Vector3(SelectionBoxPrefab.SelectorSideFaces[i].transform.localScale.x * value.x, SelectionBoxPrefab.SelectorSideFaces[i].transform.localScale.y * value.y, SelectionBoxPrefab.SelectorSideFaces[i].transform.localScale.z * value.x);
            }

            for (int i = 0; i < SelectorTopBottomFaces.Count; i++)
            {
                SelectorTopBottomFaces[i].transform.localPosition = new Vector3(SelectionBoxPrefab.SelectorTopBottomFaces[i].transform.localPosition.x * value.x, SelectionBoxPrefab.SelectorTopBottomFaces[i].transform.localPosition.y * value.y, SelectionBoxPrefab.SelectorTopBottomFaces[i].transform.localPosition.z * value.x);
                SelectorTopBottomFaces[i].transform.localScale = new Vector3(SelectionBoxPrefab.SelectorTopBottomFaces[i].transform.localScale.x * value.x, SelectionBoxPrefab.SelectorTopBottomFaces[i].transform.localScale.y * value.x, SelectionBoxPrefab.SelectorTopBottomFaces[i].transform.localScale.z * value.y);
            }
        }
    }
}
