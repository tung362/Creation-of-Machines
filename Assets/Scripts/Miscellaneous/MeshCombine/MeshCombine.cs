using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM
{
    public class MeshCombine : MonoBehaviour
    {
        public MeshFilter[] Targets;
        private MeshFilter TheMeshFilter;

        void Start()
        {
            TheMeshFilter = GetComponent<MeshFilter>();
            Combine();
        }

        void Update()
        {

        }

        void Combine()
        {
            Mesh newMesh = new Mesh();

            List<int[]> tris = new List<int[]>();
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector3> uvs = new List<Vector3>();


        }
    }
}
