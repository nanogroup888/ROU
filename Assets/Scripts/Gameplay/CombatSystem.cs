using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class CombatSystem : NetworkBehaviour
    {
        // Placeholder for future: aggro, hit formulas, crits, status effects
        public static int CalcPhysicalDamage(int str, int level) => Mathf.Max(1, str * 2 + level);
    }
}
