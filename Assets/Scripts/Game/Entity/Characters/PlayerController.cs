using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using COM.Test;

namespace COM
{
    //Handles player action inputs of entity
    public class PlayerController : MonoBehaviour
    {
        public EntityBody EntityToControl;

        private Vector2Int PreviousChunkCoord = Vector2Int.zero;
        private HashSet<Vector2Int> PreviousLoadedChunkCoords = new HashSet<Vector2Int>();
        private HashSet<Vector2Int> currentLoadedChunkCoords = new HashSet<Vector2Int>();
        private int CurrentChunkX = -1;
        private int CurrentChunkY = -1;

        void Start()
        {

        }

        void Update()
        {

        }

        void FixedUpdate()
        {
            LoadChunk();
        }

        void LoadChunk()
        {
            Vector2Int chunkCoord = Vector2Int.FloorToInt(new Vector2(transform.position.x, transform.position.z) / WorldGeneratorOld.instance.MapChunkOffset);

            //Reset increment
            if (PreviousChunkCoord != chunkCoord)
            {
                CurrentChunkX = -1;
                CurrentChunkY = -1;
                currentLoadedChunkCoords = new HashSet<Vector2Int>();
                PreviousChunkCoord = chunkCoord;
            }

            //Loading a chunk in a 3x3 grid incrementally
            if (CurrentChunkX <= 1)
            {
                Vector2Int coord = new Vector2Int(chunkCoord.x + CurrentChunkX, chunkCoord.y + CurrentChunkY);
                currentLoadedChunkCoords.Add(coord);

                if (PreviousLoadedChunkCoords.Contains(coord)) PreviousLoadedChunkCoords.Remove(coord);
                else WorldGeneratorOld.instance.CreateMapChunk(coord.x, coord.y);

                CurrentChunkY++;
                if (CurrentChunkY > 1)
                {
                    CurrentChunkY = -1;
                    CurrentChunkX++;
                }
            }
            else
            {
                if(PreviousLoadedChunkCoords != currentLoadedChunkCoords)
                {
                    //Unloads chunks that are outside of the 3x3 grid incrementally
                    foreach (Vector2Int invalidCoord in PreviousLoadedChunkCoords)
                    {
                        WorldGeneratorOld.instance.DestroyMapChunk(invalidCoord.x, invalidCoord.y);
                        PreviousLoadedChunkCoords.Remove(invalidCoord);
                        break;
                    }

                    if (PreviousLoadedChunkCoords.Count == 0) PreviousLoadedChunkCoords = currentLoadedChunkCoords;
                }
            }

            ////To do: incremental
            //HashSet<Vector2Int> currentLoadedChunkCoords = new HashSet<Vector2Int>();
            ////Loads chunk in a 3x3 grid
            //for(int x = -1; x <= 1; x++)
            //{
            //    for(int z = -1; z <= 1; z++)
            //    {
            //        Vector2Int coord = new Vector2Int(chunkCoord.x + x, chunkCoord.y + z);
            //        currentLoadedChunkCoords.Add(coord);

            //        if (PreviousLoadedChunkCoords.Contains(coord)) PreviousLoadedChunkCoords.Remove(coord);
            //        else WorldGenerator.instance.CreateMapChunk(coord.x, coord.y);
            //    }
            //}

            ////Unloads chunks that are outside of the 3x3
            //foreach (Vector2Int invalidCoord in PreviousLoadedChunkCoords) WorldGenerator.instance.DestroyMapChunk(invalidCoord.x, invalidCoord.y);

            //PreviousLoadedChunkCoords = currentLoadedChunkCoords;
            //PreviousChunkCoord = chunkCoord;
        }
    }
}
