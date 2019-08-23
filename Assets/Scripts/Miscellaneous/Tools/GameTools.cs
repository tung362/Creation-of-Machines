using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GameTools
{
    /*General*/
    public static List<GameObject> FindAllChilds(Transform TheGameObject)
    {
        List<GameObject> retval = new List<GameObject>();
        foreach (Transform child in TheGameObject)
        {
            retval.Add(child.gameObject);
            retval.AddRange(FindAllChilds(child));
        }
        return retval;
    }

    public static void ApplyLayerToChilds(Transform TheGameObject, string LayerName)
    {
        foreach (Transform child in TheGameObject)
        {
            child.gameObject.layer = LayerMask.NameToLayer(LayerName);
            ApplyLayerToChilds(child, LayerName);
        }
    }

    public static List<MeshRenderer> FindAllMeshes(Transform TheGameObject)
    {
        List<MeshRenderer> retval = new List<MeshRenderer>();
        foreach (Transform child in TheGameObject)
        {
            if (child.GetComponent<MeshRenderer>() != null) retval.Add(child.GetComponent<MeshRenderer>());
            retval.AddRange(FindAllMeshes(child));
        }
        return retval;
    }

    public static List<MeshFilter> FindAllMeshFilters(Transform TheGameObject)
    {
        List<MeshFilter> retval = new List<MeshFilter>();
        foreach (Transform child in TheGameObject)
        {
            if (child.GetComponent<MeshFilter>() != null) retval.Add(child.GetComponent<MeshFilter>());
            retval.AddRange(FindAllMeshFilters(child));
        }
        return retval;
    }

    /*Nav Mesh*/
    public static NavMeshBuildSource CreateEmptySource()
    {
        NavMeshBuildSource emptySource = new NavMeshBuildSource();
        emptySource.sourceObject = new Mesh();
        return emptySource;
    }

    public static string GetAgentName(int AgentIndex)
    {
        return NavMesh.GetSettingsNameFromID(NavMesh.GetSettingsByIndex(AgentIndex).agentTypeID);
    }

    public static int GetAgentID(int AgentIndex)
    {
        return NavMesh.GetSettingsByIndex(AgentIndex).agentTypeID;
    }

#if UNITY_EDITOR
    public static string GetNavAreaName(int AreaIndex)
    {
        string[] areas = GameObjectUtility.GetNavMeshAreaNames();
        return areas[AreaIndex];
    }

    public static int GetNavAreaID(int AreaIndex)
    {
        string[] areas = GameObjectUtility.GetNavMeshAreaNames();
        return NavMesh.GetAreaFromName(areas[AreaIndex]);
    }
#endif

    //Math
    public static Vector3 GetNearestPointOnLine(Vector3 linePoint, Vector3 lineDirection, Vector3 samplePoint)
    {
        Vector3 lPToSPDirection = samplePoint - linePoint;
        float dot = Vector3.Dot(lPToSPDirection, lineDirection);
        return linePoint + lineDirection * dot;
    }

    public static Vector2 GetNearestPointOnLine(Vector2 linePoint, Vector2 lineDirection, Vector2 samplePoint)
    {
        Vector2 lPToSPDirection = samplePoint - linePoint;
        float dot = Vector2.Dot(lPToSPDirection, lineDirection);
        return linePoint + lineDirection * dot;
    }
}
