using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
//using System.Linq;

// Build and update a localized navmesh from the sources marked by NavMeshSourceTag
[DefaultExecutionOrder(-102)]
public class NavMeshRegion : MonoBehaviour
{
    /*Settings*/
    [SerializeField]
    public Transform TrackedObject;
    [SerializeField]
    public Vector3 BuildSize = new Vector3(80.0f, 20.0f, 80.0f);
    [SerializeField]
    public int AgentID = 0;
    [SerializeField]
    public float MinRegionArea = 0;
    [SerializeField]
    public bool ManualUpdate = false;

    /*Data*/
    private NavMeshData TheNavMeshData;
    private AsyncOperation Operation;
    public static bool StartManualUpdate = false;

    IEnumerator Start()
    {
        TheNavMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(TheNavMeshData);
        UpdateNavMesh(false);

        //Updates nav mesh asynchronously
        while (true)
        {
            UpdateNavMesh(true);
            yield return Operation;
        }
    }

    void UpdateNavMesh(bool asyncUpdate)
    {
        if (ManualUpdate)
        {
            if (StartManualUpdate)
            {
                Build(asyncUpdate);
                StartManualUpdate = false;
            }
        }
        else Build(asyncUpdate);
    }

    void Build(bool asyncUpdate)
    {
        Bounds bounds = QuantizedBounds();
        //Use agent settings
        NavMeshBuildSettings agentBuildSettings = NavMesh.GetSettingsByID(AgentID);
        agentBuildSettings.minRegionArea = MinRegionArea;

        //Bake updated nav mesh
        if (asyncUpdate) Operation = NavMeshBuilder.UpdateNavMeshDataAsync(TheNavMeshData, agentBuildSettings, NavMeshRegionTag.Terrains, bounds);
        else NavMeshBuilder.UpdateNavMeshData(TheNavMeshData, agentBuildSettings, NavMeshRegionTag.Terrains, bounds);
    }

    Bounds QuantizedBounds()
    {
        //Quantize the bounds to update only when theres a 10% change in size
        return new Bounds(Quantize(TrackedObject.position, 0.1f * BuildSize), BuildSize);
    }

    static Vector3 Quantize(Vector3 v, Vector3 quant)
    {
        float x = quant.x * Mathf.Floor(v.x / quant.x);
        float y = quant.y * Mathf.Floor(v.y / quant.y);
        float z = quant.z * Mathf.Floor(v.z / quant.z);
        return new Vector3(x, y, z);
    }

    void OnDrawGizmosSelected()
    {
        if (TheNavMeshData)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(TheNavMeshData.sourceBounds.center, TheNavMeshData.sourceBounds.size);
        }

        Gizmos.color = Color.yellow;
        Bounds bounds = QuantizedBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(TrackedObject.position, BuildSize);
    }
}
