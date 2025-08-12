using System.Collections.Generic;
using Mirror;
using UnityEngine;
using ROLikeMMO.Crypto;

namespace ROLikeMMO.Gameplay.NPC
{
    /// <summary>
    /// Simple on-server shop NPC.
    /// Exposes Commands that the client can call from UI to buy/sell items.
    /// Uses EconomyManager for token balance and InventorySystem.Give/Take for items.
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class ShopNPC : NetworkBehaviour
    {
        [System.Serializable]
        public class ShopItem
        {
            public string itemId;
            public int price; // in off-chain token units (e.g., LVS)
        }

        [Header("Catalog")]
        public List<ShopItem> items = new List<ShopItem>()
        {
            new ShopItem{ itemId = "potion_red", price = 50 },
            new ShopItem{ itemId = "potion_blue", price = 75 },
        };

        Dictionary<string, int> priceLut = new();

        public override void OnStartServer()
        {
            base.OnStartServer();
            priceLut.Clear();
            foreach (var it in items)
                if (!string.IsNullOrWhiteSpace(it.itemId) && it.price > 0)
                    priceLut[it.itemId] = it.price;
        }

        int GetPrice(string itemId) =>
            priceLut.TryGetValue(itemId, out var p) ? p : -1;

        // ================== Client -> Server ==================

        [Command(requiresAuthority = false)]
        public void CmdBuy(uint playerNetId, string itemId, int amount)
        {
            if (!NetworkServer.active) return;
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return;
            if (!NetworkServer.spawned.TryGetValue(playerNetId, out var ni)) return;

            var pc = ni.GetComponent<PlayerCharacter>();
            var inv = ni.GetComponent<InventorySystem>();
            var wallet = ni.GetComponent<ROLikeMMO.Crypto.OffchainWallet>();

            if (pc == null || inv == null || wallet == null) return;
            if (EconomyManager.Instance == null) return;

            int price = GetPrice(itemId);
            if (price <= 0) { TargetShopResult(pc.connectionToClient, false, "Item not sold here"); return; }

            long total = (long)price * amount;
            long bal = EconomyManager.Instance.GetBalance(pc.accountId);
            if (bal < total) { TargetShopResult(pc.connectionToClient, false, "Insufficient funds"); return; }

            // Deduct then give item
            bool burned = EconomyManager.Instance.BurnFrom(pc.accountId, total);
            if (!burned) { TargetShopResult(pc.connectionToClient, false, "Payment failed"); return; }

            bool given = inv.Give(itemId, amount);
            if (!given)
            {
                // refund if failed
                EconomyManager.Instance.MintTo(pc.accountId, total);
                TargetShopResult(pc.connectionToClient, false, "Inventory full/failed");
                return;
            }

            // Update wallet sync var
            wallet.balance = EconomyManager.Instance.GetBalance(pc.accountId);
            TargetShopResult(pc.connectionToClient, true, $"Bought {amount} x {itemId}");
        }

        [Command(requiresAuthority = false)]
        public void CmdSell(uint playerNetId, string itemId, int amount)
        {
            if (!NetworkServer.active) return;
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return;
            if (!NetworkServer.spawned.TryGetValue(playerNetId, out var ni)) return;

            var pc = ni.GetComponent<PlayerCharacter>();
            var inv = ni.GetComponent<InventorySystem>();
            var wallet = ni.GetComponent<ROLikeMMO.Crypto.OffchainWallet>();

            if (pc == null || inv == null || wallet == null) return;
            if (EconomyManager.Instance == null) return;

            int price = GetPrice(itemId);
            if (price <= 0) { TargetShopResult(pc.connectionToClient, false, "Item not buyable"); return; }

            bool taken = inv.Take(itemId, amount);
            if (!taken) { TargetShopResult(pc.connectionToClient, false, "Not enough items"); return; }

            long total = (long)price * amount;
            EconomyManager.Instance.MintTo(pc.accountId, total);
            wallet.balance = EconomyManager.Instance.GetBalance(pc.accountId);
            TargetShopResult(pc.connectionToClient, true, $"Sold {amount} x {itemId}");
        }

        // ================== Server -> Client ==================
        [TargetRpc]
        void TargetShopResult(NetworkConnection conn, bool ok, string message)
        {
            // ตรงนี้ UI ฝั่งคลายเอนต์สามารถแสดง Toast/Log ได้
            Debug.Log($"[ShopNPC] {(ok ? "OK" : "FAIL")} : {message}");
        }
    }
}
