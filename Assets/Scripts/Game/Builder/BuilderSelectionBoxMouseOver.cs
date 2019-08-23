using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderSelectionBoxMouseOver : MonoBehaviour
{
    public Material MouseOverMaterial;
    public Material NormalMaterial;

    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void OnMouseEnter()
    {
        meshRenderer.material = MouseOverMaterial;
        Debug.Log("Enter");
    }

    void OnMouseExit()
    {
        meshRenderer.material = NormalMaterial;
        Debug.Log("Exit");
    }
}
