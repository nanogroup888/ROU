using Logger = ROLikeMMO.Util.Logger;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using ROLikeMMO.Persistence;
using ROLikeMMO.Util;

namespace ROLikeMMO.Crypto
{
    public class EconomyManager : NetworkBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Token Config")]
        public string tokenSymbol = "LVS";
        public long maxSupply = 1_000_000_000; // 1B units
        public long circulatingSupply = 0;

        private readonly Dictionary<string, long> balances = new(); // accountId -> balance

        void Awake() { Instance = this; }

        public override void OnStartServer()
        {
            base.OnStartServer();
            var world = DataStorage.LoadWorld();
            circulatingSupply = 0;
            if (world.treasury.TryGetValue("circulating", out var circ))
                circulatingSupply = circ;

            Logger.Log($"Economy started. Circ={circulatingSupply}");
        }

        [Server]
        public bool MintTo(string accountId, long amount)
        {
            if (amount <= 0) return false;
            if (circulatingSupply + amount > maxSupply) return false;
            balances.TryGetValue(accountId, out var bal);
            balances[accountId] = bal + amount;
            circulatingSupply += amount;
            PersistWorld();
            return true;
        }

        [Server]
        public bool BurnFrom(string accountId, long amount)
        {
            if (!balances.TryGetValue(accountId, out var bal) || bal < amount) return false;
            balances[accountId] = bal - amount;
            circulatingSupply -= amount;
            PersistWorld();
            return true;
        }

        [Server]
        public bool TryTransfer(string from, string to, long amount)
        {
            if (amount <= 0) return false;
            balances.TryGetValue(from, out var bf);
            if (bf < amount) return false;
            balances[from] = bf - amount;
            balances.TryGetValue(to, out var bt);
            balances[to] = bt + amount;
            return true;
        }

        [Server]
        public long GetBalance(string accountId)
        {
            balances.TryGetValue(accountId, out var bal);
            return bal;
        }

        void PersistWorld()
        {
            var w = DataStorage.LoadWorld();
            w.treasury["circulating"] = (int)circulatingSupply;
            w.lastSavedISO = System.DateTime.UtcNow.ToString("o");
            DataStorage.SaveWorld(w);
        }
    }
}
