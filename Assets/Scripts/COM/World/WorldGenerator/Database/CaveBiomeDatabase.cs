using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Malee.List;

namespace COM.Database.World
{
    [CreateAssetMenu(menuName = "Creation of Machines/Cave Biome Database", fileName = "CaveBiomeDatabase.asset")]
    public class CaveBiomeDatabase : ScriptableObject, ISerializationCallbackReceiver
    {
        #region Format
        [System.Serializable]
        public class CaveBiomeList : ReorderableArray<CaveBiome> { }

        [System.Serializable]
        public class CaveBiome
        {
            //Id
            public string Name;
            //Color
            public Gradient GroundPalette;
            public Gradient WallPalette;
            //General
            public float Persistance;
            //Cave layer
            public float Threshold;
            //Additive
            //Subtractive
        }
        #endregion

        [Reorderable]
        public CaveBiomeList CaveBiomes = new CaveBiomeList();

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
            CaveBiomes = new CaveBiomeList();

            //Save
            if (save) Save();
        }
    }
}
