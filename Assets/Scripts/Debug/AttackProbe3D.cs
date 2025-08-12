using Mirror;
using UnityEngine;

namespace ROLikeMMO.DebugTools
{
    /// <summary>
    /// Press 'K' to force-attack the nearest monster (3D only).
    /// Attach to Player prefab for quick sanity checks.
    /// </summary>
    [RequireComponent(typeof(ROLikeMMO.Gameplay.PlayerCharacter))]
    public class AttackProbe3D : NetworkBehaviour
    {
        public float searchRadius = 60f;
        public LayerMask monsterMask; // tick "Monster"

        void Update()
        {
            if (!isLocalPlayer) return;
            if (Input.GetKeyDown(KeyCode.K))
            {
                var best = FindNearestByCollider();
                if (best == null)
                {
                    Debug.Log("[AttackProbe3D] No monster found. Check layer/collider/spawnable prefabs.");
                    return;
                }
                var ni = best.GetComponentInParent<NetworkIdentity>();
                if (ni == null)
                {
                    Debug.Log("[AttackProbe3D] Monster root missing NetworkIdentity.");
                    return;
                }
                var pc = GetComponent<ROLikeMMO.Gameplay.PlayerCharacter>();
                Debug.Log($"[AttackProbe3D] Request attack netId={ni.netId} target={best.name}");
                pc.CmdAttackTarget(ni.netId);
            }
        }

        Transform FindNearestByCollider()
        {
            int mask = monsterMask.value == 0 ? ~0 : monsterMask.value;
            var hits = Physics.OverlapSphere(transform.position, searchRadius, mask, QueryTriggerInteraction.Collide);
            Transform best = null;
            float bestDist = float.MaxValue;
            foreach (var h in hits)
            {
                float d = (h.transform.position - transform.position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = h.transform; }
            }
            return best;
        }
    }
}
