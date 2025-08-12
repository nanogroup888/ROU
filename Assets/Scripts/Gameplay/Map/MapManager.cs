using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    [Serializable]
    public class MapDef
    {
        public string id;
        public string scene;
        public string name;
    }

    public class MapManager : MonoBehaviour
    {
        public static Dictionary<string, MapDef> Maps = new();

        public TextAsset mapsJson;

        void Awake()
        {
            if (mapsJson != null)
            {
                var wrap = JsonUtility.FromJson<MapArray>(mapsJson.text);
                Maps.Clear();
                foreach (var m in wrap.maps) Maps[m.id] = m;
            }
        }

        [Serializable] class MapArray { public MapDef[] maps; }
    }
}
