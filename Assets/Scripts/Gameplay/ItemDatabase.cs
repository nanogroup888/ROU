using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    [Serializable]
    public class ItemDef
    {
        public uint id;
        public string name;
        public ItemType type;
        public EquipSlot equipSlot;
        public int atk;
        public int def;
        public int healHp;
        public int healMp;
        public int price;
        public string desc;
    }

    public static class ItemDatabase
    {
        static Dictionary<uint, ItemDef> _cache;
        public static IReadOnlyDictionary<uint, ItemDef> All => _cache;
        public static ItemDef Get(uint id) => All != null && All.TryGetValue(id, out var d) ? d : null;

        public static void LoadFromJson(TextAsset jsonAsset)
        {
            var wrapper = JsonUtility.FromJson<ItemDefArrayWrapper>(jsonAsset.text);
            _cache = new Dictionary<uint, ItemDef>();
            foreach (var it in wrapper.items) _cache[it.id] = it;
        }

        [Serializable] class ItemDefArrayWrapper { public ItemDef[] items; }
    }
}
