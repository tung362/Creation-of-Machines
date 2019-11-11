﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM.Test
{
    //Game manager, keeps track of all registries
    public class RunOld : MonoBehaviour
    {
        public static RunOld instance { get; private set; }

        public OuterTileDictionary OuterTiles;
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

        //Voronoi data
        private List<Vector2> RegionSiteCoords;


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