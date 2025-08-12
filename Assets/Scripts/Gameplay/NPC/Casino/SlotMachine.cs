using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class SlotMachine : NetworkBehaviour
    {
        [Server]
        public void ServerSpin(PlayerCharacter pc, long bet)
        {
            // 1% jackpot 10x, 10% 2x, else lose
            float r = Random.value;
            var casino = FindObjectOfType<CasinoManager>();
            if (r < 0.01f) casino.Payout(pc, bet * 10);
            else if (r < 0.11f) casino.Payout(pc, bet * 2);
        }
    }
}
