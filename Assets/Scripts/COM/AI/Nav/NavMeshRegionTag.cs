using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
//using System.Linq;
//using System.Collections.Specialized;
//using System.Collections;
using COM.Utils;

namespace COM.AI
{
    public enum BuildTypeEnum { Mesh, Terrain, Collider }

    [DefaultExecutionOrder(-200)]
    public class NavMeshRegionTag : MonoBehaviour
    {
        /*Settings*/
        [SerializeField]
        public BuildTypeEnum BuildType = BuildTypeEnum.Mesh;
        [SerializeField]
        public int AreaID = 0;

        /*Data*/
        private NavMeshBuildSource CurrentSource = new NavMeshBuildSource();
        private int SourceIndex = -1;
        public static List<NavMeshBuildSource> Terrains = new List<NavMeshBuildSource>();
        public static List<int> UnusedIndexes = new List<int>();
        public static bool HasGeneratedDummySource = false;

        /*Components*/
        private MeshFilter TheMeshFilter;
        private Terrain TheTerrain;
        private Collider TheCollider;

        void OnEnable()
        {
            TheMeshFilter = GetComponent<MeshFilter>();
            TheTerrain = GetComponent<Terrain>();
            TheCollider = GetComponent<Collider>();

            if (!HasGeneratedDummySource)
            {
                Terrains.Add(GameTools.CreateEmptySource());
                HasGeneratedDummySource = true;
            }

            Collect();
        }

        void OnDestroy()
        {
            if (SourceIndex != -1)
            {
                Terrains.RemoveAt(SourceIndex);
                UnusedIndexes.Add(SourceIndex);
                Terrains.Insert(SourceIndex, GameTools.CreateEmptySource());
                NavMeshRegion.StartManualUpdate = true;
                //Debug.Log("Destroyed: " + SourceIndex);
            }
        }

        //Call on this to update any nav mesh changes to this object
        public void Collect()
        {
            if (SourceIndex != -1) Terrains.RemoveAt(SourceIndex);

            switch (BuildType)
            {
                case BuildTypeEnum.Mesh:
                    CollectMesh();
                    break;
                case BuildTypeEnum.Terrain:
                    CollectTerrain();
                    break;
                case BuildTypeEnum.Collider:
                    CollectCollider();
                    break;
            }

            //If first executed
            if (SourceIndex == -1)
            {
                //If there is no unused indexes
                if (UnusedIndexes.Count == 0)
                {
                    //Increase list
                    Terrains.Add(CurrentSource);
                    SourceIndex = Terrains.Count - 1;
                    //Debug.Log("Created: " + SourceIndex);
                }
                else
                {
                    //Use an unused spot in the list (recycle)
                    Terrains.RemoveAt(UnusedIndexes[0]);
                    Terrains.Insert(UnusedIndexes[0], CurrentSource);
                    SourceIndex = UnusedIndexes[0];
                    UnusedIndexes.RemoveAt(0);
                    //Debug.Log("Recycled: " + SourceIndex);
                }
            }
            else Terrains.Insert(SourceIndex, CurrentSource);
            NavMeshRegion.StartManualUpdate = true;
        }

        void CollectMesh()
        {
            if (TheMeshFilter == null) return;

            //Build Settings
            CurrentSource.shape = NavMeshBuildSourceShape.Mesh;
            CurrentSource.sourceObject = TheMeshFilter.sharedMesh;
            CurrentSource.transform = transform.localToWorldMatrix;
            CurrentSource.area = AreaID;
        }

        void CollectTerrain()
        {
            if (TheTerrain == null) return;

            //Build Settings
            CurrentSource.shape = NavMeshBuildSourceShape.Terrain;
            CurrentSource.sourceObject = TheTerrain.terrainData;
            //Terrain system only supports translation - so we pass translation only to back-end
            CurrentSource.transform = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
            CurrentSource.area = AreaID;
        }

        void CollectCollider()
        {
            if (TheCollider == null) return;

            //Build Settings
            CurrentSource.component = TheCollider;

            if (TheCollider.GetType() == typeof(BoxCollider))
            {
                BoxCollider theBoxCollider = TheCollider as BoxCollider;
                CurrentSource.shape = NavMeshBuildSourceShape.Box;

                Vector3 center = transform.TransformPoint(theBoxCollider.center);
                Vector3 scale = transform.lossyScale;
                Vector3 size = new Vector3(theBoxCollider.size.x * Mathf.Abs(scale.x), theBoxCollider.size.y * Mathf.Abs(scale.y), theBoxCollider.size.z * Mathf.Abs(scale.z));

                CurrentSource.transform = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
                CurrentSource.size = size;
            }
            else if (TheCollider.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider theCapsuleCollider = TheCollider as CapsuleCollider;
                CurrentSource.shape = NavMeshBuildSourceShape.Capsule;

                Vector3 center = transform.TransformPoint(theCapsuleCollider.center);
                Vector3 scale = transform.lossyScale;
                Vector3 size = new Vector3(theCapsuleCollider.radius * Mathf.Abs(scale.x), theCapsuleCollider.height * Mathf.Abs(scale.y), theCapsuleCollider.radius * Mathf.Abs(scale.z));

                CurrentSource.transform = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
                CurrentSource.size = size;
            }
            else if (TheCollider.GetType() == typeof(SphereCollider))
            {
                SphereCollider theSphereCollider = TheCollider as SphereCollider;
                CurrentSource.shape = NavMeshBuildSourceShape.Sphere;

                Vector3 center = transform.TransformPoint(theSphereCollider.center);
                Vector3 scale = transform.lossyScale;
                Vector3 size = new Vector3(theSphereCollider.radius * Mathf.Abs(scale.x), theSphereCollider.radius * Mathf.Abs(scale.y), theSphereCollider.radius * Mathf.Abs(scale.z));

                CurrentSource.transform = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
                CurrentSource.size = size;
            }
            else if (TheCollider.GetType() == typeof(MeshCollider))
            {
                MeshCollider theMeshCollider = TheCollider as MeshCollider;
                CurrentSource.shape = NavMeshBuildSourceShape.Mesh;

                CurrentSource.sourceObject = theMeshCollider.sharedMesh;
                CurrentSource.transform = transform.localToWorldMatrix;
            }
            CurrentSource.area = AreaID;
        }
    }
}
