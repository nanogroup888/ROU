using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class Blackjack : NetworkBehaviour
    {
        // Minimal stub for networking hooks
        [Server]
        public void ServerPlayRound(PlayerCharacter pc, long bet)
        {
            // Very simplified: 48% chance player wins 2x
            if (Random.value < 0.48f)
            {
                var casino = FindObjectOfType<CasinoManager>();
                casino.Payout(pc, bet * 2);
            }
        }
    }
}
