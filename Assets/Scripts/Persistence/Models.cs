using System;
using System.Collections.Generic;
using UnityEngine;

namespace ROLikeMMO.Persistence
{
    [Serializable]
    public class PlayerSave
    {
        public string accountId;
        public string characterName;
        public string mapId = "Field_Pronto";
        public float x, y, z;
        public int level = 1;
        public int jobLevel = 1;
        public int exp = 0;
        public int jobExp = 0;
        public int statStr = 1, statAgi = 1, statVit = 1, statInt = 1, statDex = 1, statLuk = 1;
        public int hp = 50, mp = 30, hpMax = 50, mpMax = 30;
        public List<uint> inventoryItemIds = new();
        public List<int> inventoryQty = new();
        public List<uint> equipmentSlots = new(); // by slot index -> itemId(0 = empty)
        public long walletBalance = 0;
    }

    [Serializable]
    public class WorldSave
    {
        public string lastSavedISO;
        public Dictionary<string,int> monsterCounters = new();
        public Dictionary<string,long> treasury = new();
    }
}
