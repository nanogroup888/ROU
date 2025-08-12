using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    /// Backward-compat for old code calling Damage/Heal/SetMax.
    public static class HealthExtensions
    {
        public static void Damage(this Health hp, int dmg)
        {
            if (hp == null) return;
            if (NetworkServer.active) hp.ServerTakeDamage(dmg, null);
            else Debug.LogWarning("[Health.Damage] called on client. Ignored.");
        }

        public static void Damage(this Health hp, int dmg, NetworkIdentity attacker)
        {
            if (hp == null) return;
            if (NetworkServer.active) hp.ServerTakeDamage(dmg, attacker);
            else Debug.LogWarning("[Health.Damage(attacker)] called on client. Ignored.");
        }

        public static void Heal(this Health hp, int amount)
        {
            if (hp == null) return;
            if (NetworkServer.active) hp.ServerHeal(amount);
            else Debug.LogWarning("[Health.Heal] called on client. Ignored.");
        }

        public static void SetMax(this Health hp, int newMax)
        {
            if (hp == null) return;
            if (NetworkServer.active) hp.SetMax(newMax, true);
            else Debug.LogWarning("[Health.SetMax] called on client. Ignored.");
        }

        public static void SetMax(this Health hp, int newMax, bool refill)
        {
            if (hp == null) return;
            if (NetworkServer.active) hp.SetMax(newMax, refill);
            else Debug.LogWarning("[Health.SetMax(refill)] called on client. Ignored.");
        }
    }
}
