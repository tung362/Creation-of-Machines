using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Malee.List;
using COM.World;

namespace COM.Database.World
{
    /// <summary>
    /// Database of all registered surface biomes
    /// </summary>
    [CreateAssetMenu(menuName = "Creation of Machines/Surface Biome Database", fileName = "SurfaceBiomeDatabase.asset")]
    public class SurfaceBiomeDatabase : ScriptableObject, ISerializationCallbackReceiver
    {
        #region Format
        [System.Serializable]
        public class SurfaceBiomeList : ReorderableArray<SurfaceBiome> { }

        [System.Serializable]
        public class SurfaceBiome
        {
            //Id
            public string Name;
            //Color
            public Gradient GroundPalette;
            public Gradient WallPalette;
            //General
            public float Persistance;
            //Surface layer
            public float Height;
            public float Floor;
            //Additive layer
            public float AdditiveHeight;
            public float AdditiveHeightLimit;
            public float AdditiveOffset;
            //Subtractive
            //Environment
            [ColorUsage(true, true)]
            public Color DayColor;
            [ColorUsage(true, true)]
            public Color NightColor;
            public List<WeatherEvent> WeatherEvents;
        }
        #endregion

        [Reorderable]
        public SurfaceBiomeList Biomes = new SurfaceBiomeList();

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {

        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void ClearDatabase(bool save = true)
        {
            Biomes = new SurfaceBiomeList();

            //Save
            if (save) Save();
        }
    }
}
