using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerCharacter))]
    [RequireComponent(typeof(NetworkIdentity))]
    public class InventorySystem : NetworkBehaviour
    {
        [Serializable]
        public struct ItemStack
        {
            public string itemId;
            public int amount;
        }

        public class SyncListItemStack : SyncList<ItemStack> { }

        public readonly SyncListItemStack items = new SyncListItemStack();

        PlayerCharacter pc;

        string OwnerAccountId => pc != null ? pc.accountId : string.Empty;
        string SavePath =>
            Path.Combine(Application.persistentDataPath, "ROLikeMMO", "players", $"{OwnerAccountId}_inventory.json");

        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(ServerInitSafe());
        }

        IEnumerator ServerInitSafe()
        {
            pc = GetComponent<PlayerCharacter>();
            while (pc == null)
            {
                yield return null;
                pc = GetComponent<PlayerCharacter>();
            }

            // ถ้ายังไม่มี accountId ให้รอจนกว่าจะถูกตั้ง (ไม่ timeout)
            yield return StartCoroutine(WaitForAccountId());

            // โหลดอินเวนทอรีครั้งแรกเมื่อ account พร้อม
            LoadFromDisk();
        }

        IEnumerator WaitForAccountId()
        {
            while (string.IsNullOrWhiteSpace(OwnerAccountId))
                yield return null;
        }

        /// <summary>
        /// เรียกจากเซิร์ฟเวอร์ได้ เมื่อแน่ใจว่า accountId ถูกตั้งแล้ว
        /// </summary>
        [Server]
        public void EnsureLoaded()
        {
            if (string.IsNullOrWhiteSpace(OwnerAccountId)) return;
            if (!_loadedOnce) LoadFromDisk();
        }

        bool _loadedOnce = false;

        // ---------- Persistence ----------
        [Server]
        void LoadFromDisk()
        {
            if (_loadedOnce) return;
            _loadedOnce = true;

            items.Clear();

            try
            {
                var dir = Path.GetDirectoryName(SavePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (!File.Exists(SavePath))
                {
                    SaveToDisk(); // เขียนไฟล์เปล่าไว้ก่อน
                    return;
                }

                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<ItemStackArrayWrapper>(json);
                if (data?.items != null)
                {
                    foreach (var it in data.items)
                        items.Add(it);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Inventory] Load failed: {e.Message}");
            }
        }

        [Server]
        void SaveToDisk()
        {
            if (string.IsNullOrWhiteSpace(OwnerAccountId)) return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                var data = new ItemStackArrayWrapper { items = items.ToArray() };
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Inventory] Save failed: {e.Message}");
            }
        }

        [Serializable]
        class ItemStackArrayWrapper { public ItemStack[] items; }

        // ---------- Server API ----------
        [Server]
        public bool Give(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return false;
            if (!_loadedOnce) LoadFromDisk();

            int idx = items.FindIndex(s => s.itemId == itemId);
            if (idx >= 0)
            {
                var st = items[idx];
                st.amount += amount;
                items[idx] = st;
            }
            else
            {
                items.Add(new ItemStack { itemId = itemId, amount = amount });
            }

            SaveToDisk();
            return true;
        }

        [Server]
        public bool Take(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return false;
            if (!_loadedOnce) LoadFromDisk();

            int idx = items.FindIndex(s => s.itemId == itemId);
            if (idx < 0) return false;

            var st = items[idx];
            if (st.amount < amount) return false;

            st.amount -= amount;
            if (st.amount == 0) items.RemoveAt(idx);
            else items[idx] = st;

            SaveToDisk();
            return true;
        }
    }
}
