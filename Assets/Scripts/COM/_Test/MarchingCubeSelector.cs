using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.Test
{
    public class MarchingCubeSelector : MonoBehaviour
    {
        public LayerMask SelectionLayer;
        public Material OnMat;
        public Material OffMat;
        public float IsoLevel = 8;
        int NumOfCubesPerAxis = 30;
        public float ChunkSize = 1;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit[] selectionHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, SelectionLayer);

                for(int i = 0; i < selectionHits.Length; i++)
                {
                    MarchingCubeTest marchingCubeTest = selectionHits[i].transform.parent.parent.GetComponent<MarchingCubeTest>();
                    MarchingCubeGridNode gridNode = selectionHits[i].transform.GetComponent<MarchingCubeGridNode>();

                    gridNode.NodeValue = gridNode.NodeValue == 0 ? 1 : 0;
                    selectionHits[i].transform.GetComponent<MeshRenderer>().material = gridNode.NodeValue == 0 ? OffMat : OnMat;
                    marchingCubeTest.GenerateMesh();
                }
            }
        }
    }
}
