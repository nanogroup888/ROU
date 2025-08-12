using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class ExperienceSystem : NetworkBehaviour
    {
        public static int ExpForLevel(int lv) => Mathf.FloorToInt(50 * Mathf.Pow(1.1f, lv));
    }
}
