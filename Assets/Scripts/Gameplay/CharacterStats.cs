using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class CharacterStats : NetworkBehaviour
    {
        [SyncVar] public int level = 1;
        [SyncVar] public int jobLevel = 1;
        [SyncVar] public int exp = 0;
        [SyncVar] public int jobExp = 0;

        [SyncVar] public int str = 1, agi = 1, vit = 1, _int = 1, dex = 1, luk = 1;
        [SyncVar] public int hp = 50, mp = 30, hpMax = 50, mpMax = 30;
        [SyncVar] public float atkSpeed = 1.0f, moveSpeed = 4.0f;

        public void Recalculate()
        {
            hpMax = 40 + vit * 10 + level * 5;
            mpMax = 20 + _int * 8 + level * 3;
            atkSpeed = Mathf.Clamp(1.5f - (agi * 0.01f), 0.3f, 1.5f);
            moveSpeed = 4.0f + Mathf.Min(2.0f, agi * 0.02f);
            hp = Mathf.Clamp(hp, 0, hpMax);
            mp = Mathf.Clamp(mp, 0, mpMax);
        }
    }
}
