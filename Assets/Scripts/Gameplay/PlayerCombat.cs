using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    /// Server-authoritative player combat.
    [DisallowMultipleComponent]
    public class PlayerCombat : NetworkBehaviour
    {
        public int baseDamage = 8;
        public float maxAttackDistance = 10f;
        public float attackCooldown = 0.5f;
        double nextAttackTime;

        [Command]
        public void CmdAttackTarget(uint targetNetId)
        {
            if (!NetworkServer.active) return;
            if (NetworkTime.time < nextAttackTime) return;
            nextAttackTime = NetworkTime.time + attackCooldown;

            if (!NetworkServer.spawned.TryGetValue(targetNetId, out var targetNI)) return;

            float dist = Vector3.Distance(transform.position, targetNI.transform.position);
            if (dist > maxAttackDistance) return;

            var hp = targetNI.GetComponent<Health>();
            if (hp != null) hp.ServerTakeDamage(baseDamage, connectionToClient?.identity);
        }
    }
}
