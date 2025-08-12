using System;
using System.IO;
using Mirror;
using UnityEngine;
using ROLikeMMO.Persistence; // expects PlayerSave to exist

namespace ROLikeMMO.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : NetworkBehaviour
    {
        [Header("Identity")]
        public string accountId;
        public string characterName;

        [Header("Combat")]
        public float attackRange = 3.0f;
        public int attackDamage = 7;
        public float attackCooldown = 0.6f;
        float lastAttackTime;

        Animator anim;
        CharacterController cc;

        // ---------- Persistence ----------
        [NonSerialized] public PlayerSave SaveData;

        string SavePath =>
            Path.Combine(Application.persistentDataPath, "ROLikeMMO", "players", $"{accountId}_player.json");

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            anim = GetComponentInChildren<Animator>();
        }

        // ====== PERSISTENCE API ======
        [Server]
        public void LoadFromSave(PlayerSave save)
        {
            if (save == null) save = new PlayerSave();
            SaveData = save;

            if (!string.IsNullOrWhiteSpace(save.accountId)) accountId = save.accountId;
            if (!string.IsNullOrWhiteSpace(save.characterName)) characterName = save.characterName;

            // (ตำแหน่ง/แผนที่จะถูกจัดการที่ NetworkManager ระหว่างเปลี่ยนซีน)
        }

        [Server]
        public void SaveToStorage()
        {
            try
            {
                if (SaveData == null) SaveData = new PlayerSave();
                SaveData.accountId = accountId;
                SaveData.characterName = characterName;

                var dir = Path.GetDirectoryName(SavePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var json = JsonUtility.ToJson(SaveData, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerCharacter] Save failed: {e.Message}");
            }
        }

        // ====== TELEPORT ======
        [ClientRpc]
        public void RpcTeleport(Vector3 pos, string sceneName)
        {
            // แค่ย้ายตำแหน่งบนคลายเอนต์ (การโหลดซีนจัดการโดย NetworkManager)
            transform.position = pos;
        }

        // ====== ATTACK PIPELINE ======
        [Command]
        public void CmdAttackTarget(uint targetNetId)
        {
            if (!NetworkServer.active) return;
            if (Time.time - lastAttackTime < attackCooldown) return;

            if (!NetworkServer.spawned.TryGetValue(targetNetId, out var targetIdentity) || targetIdentity == null)
            {
                Debug.LogWarning($"[Attack] targetNetId {targetNetId} not found");
                return;
            }

            var targetGO = targetIdentity.gameObject;
            float d = Vector3.Distance(transform.position, targetGO.transform.position);
            if (d > attackRange + 0.25f)
            {
                Debug.Log($"[Attack] too far {d:F2}>{attackRange}");
                return;
            }

            lastAttackTime = Time.time;

            var health = targetGO.GetComponent<Health>();
            if (health != null) health.Damage(attackDamage);
            else Debug.LogWarning("[Attack] Target has no Health component.");

            // Show damage numbers for all clients
            RpcShowDamageAt(targetGO.transform.position + Vector3.up * 1.1f, attackDamage, false);

            // Play attack animation on the attacker (owner)
            TargetOnAttack(connectionToClient);
        }

        [TargetRpc] void TargetOnAttack(NetworkConnection conn) { if (anim) anim.SetTrigger("Attack"); }
        [ClientRpc] void RpcShowDamageAt(Vector3 pos, int dmg, bool crit)
        {
            ROLikeMMO.UI.DamagePopupManager.Instance?.ShowDamageWorld(pos, dmg, crit);
        }
    }
}
