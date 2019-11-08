using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.Test
{
    public class WorldGeneratorPool : MonoBehaviour
    {
        public static WorldGeneratorPool instance { get; private set; }

        public Dictionary<string, GameObjectPool> Pools = new Dictionary<string, GameObjectPool>();

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"WorldGeneratorPool\"");
        }

        public GameObject TakeFromPool(GameObject prefab, Transform parent = null)
        {
            if(Pools.ContainsKey(prefab.name))
            {
                if (Pools[prefab.name].UnUsed.Count == 0) return CreateNewObjectToThePool(prefab, parent);
                else
                {
                    GameObject retval = null;
                    foreach (GameObject unused in Pools[prefab.name].UnUsed)
                    {
                        retval = unused;
                        Pools[prefab.name].Used.Add(unused);
                        Pools[prefab.name].UnUsed.Remove(unused);
                        break;
                    }
                    return retval;
                }
            }
            else
            {
                Pools.Add(prefab.name, new GameObjectPool());
                return CreateNewObjectToThePool(prefab, parent);
            }
        }

        public void GiveBackToPool(GameObject poolGameObject)
        {
            if(Pools.ContainsKey(poolGameObject.name))
            {
                poolGameObject.SetActive(false);
                Pools[poolGameObject.name].UnUsed.Add(poolGameObject);
                Pools[poolGameObject.name].Used.Remove(poolGameObject);
            }
            else Debug.Log("Warning! Game Object: " + poolGameObject.name + " is not part of any pools");
        }

        public GameObject CreateNewObjectToThePool(GameObject prefab, Transform parent = null)
        {
            GameObject placementTile;
            if(!parent) placementTile = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            else placementTile = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);

            placementTile.name = prefab.name;
            Pools[prefab.name].Used.Add(placementTile);
            return placementTile;
        }
    }
}
