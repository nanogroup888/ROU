using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    /// <summary>
    /// Small hook so MonsterAI talks to AnimatorParamAdapter if present.
    /// </summary>
    public partial class MonsterAI : NetworkBehaviour
    {
        AnimatorParamAdapter adapter;

        void Start()
        {
            adapter = GetComponentInChildren<AnimatorParamAdapter>();
        }

        [ClientRpc] public void RpcSetSpeed(float s)
        {
            if (adapter) adapter.SetSpeed(s);
        }

        [ClientRpc] public void RpcHit()
        {
            if (adapter) adapter.PlayHit();
        }

        [ClientRpc] public void RpcDie()
        {
            if (adapter) adapter.PlayDie();
        }

        [ClientRpc] public void RpcAttack()
        {
            if (adapter) adapter.PlayAttack();
        }
    }
}
