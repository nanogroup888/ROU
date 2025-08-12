using Mirror;
using UnityEngine;

namespace ROLikeMMO.DebugTools
{
    /// <summary>
    /// Press 'K' to force-attack the nearest monster.
    /// Works with 3D (Collider) and optionally 2D (Collider2D).
    /// Use Layer "Monster" OR attach MonsterAI on the target.
    /// </summary>
    [RequireComponent(typeof(ROLikeMMO.Gameplay.PlayerCharacter))]
    public class AttackProbe : NetworkBehaviour
    {
        public float searchRadius = 40f;
        public LayerMask monsterMask3D = ~0;
        public bool search2D = false;
        public LayerMask monsterMask2D = ~0;

        void Update()
        {
            if (!isLocalPlayer) return;

            if (Input.GetKeyDown(KeyCode.K))
            {
                var target = FindNearestMonsterGO();
                if (target == null)
                {
                    Debug.Log("[AttackProbe] No monster found. Check: (1) Layer=Monster or has MonsterAI, (2) Collider present, (3) within radius, (4) prefab spawned.");
                    return;
                }

                var ni = target.GetComponentInParent<NetworkIdentity>();
                if (ni == null)
                {
                    Debug.Log("[AttackProbe] Monster has no NetworkIdentity on root. Add NetworkIdentity to the prefab's root.");
                    return;
                }

                var pc = GetComponent<ROLikeMMO.Gameplay.PlayerCharacter>();
                Debug.Log($"[AttackProbe] Request attack netId={ni.netId} target={target.name}");
                pc.CmdAttackTarget(ni.netId);
            }
        }

        GameObject FindNearestMonsterGO()
        {
            GameObject best = null;
            float bestDist = float.MaxValue;

            // 3D search by layer
            var hits = Physics.OverlapSphere(transform.position, searchRadius, monsterMask3D, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                float d = (h.transform.position - transform.position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject; }
            }

            // Fallback by component (MonsterAI) if layer didn't catch
            var all = GameObject.FindObjectsOfType<ROLikeMMO.Gameplay.MonsterAI>();
            foreach (var ai in all)
            {
                float d = (ai.transform.position - transform.position).sqrMagnitude;
                if (d < searchRadius * searchRadius && d < bestDist)
                {
                    bestDist = d;
                    best = ai.gameObject;
                }
            }

            // Optional 2D search
            if (search2D)
            {
                var hits2D = Physics2D.OverlapCircleAll(transform.position, searchRadius, monsterMask2D);
                foreach (var h in hits2D)
                {
                    float d = (h.transform.position - transform.position).sqrMagnitude;
                    if (d < bestDist) { bestDist = d; best = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject; }
                }
            }

            return best;
        }
    }
}
