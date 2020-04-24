using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM
{
    public class Run : MonoBehaviour
    {
        public static Run instance { get; private set; }

        /*Generation*/
        public List<RegionSurfaceBiome> SurfaceBiomes = new List<RegionSurfaceBiome>();
        public List<RegionCaveBiome> CaveBiomes = new List<RegionCaveBiome>();

        //HouseDecorations (tables, lights, chests, etc)
        //Terrain tiles
        //Terrain objects (rocks and plants)
        //Terrain underground tiles

        //Weapons
        //Armors
        //Accessories
        //Consumables
        //Crafting materials
        //Miscellaneous

        //Races

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.Log("Warning! Multiple instances of \"Run\"");
        }

        void Start()
        {
            GameSettings.Load();
            OptionSettings.Load();
        }

        void Update()
        {

        }
    }
}
