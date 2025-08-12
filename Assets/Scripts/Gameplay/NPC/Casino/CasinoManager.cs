using Mirror;
using UnityEngine;
using ROLikeMMO.Crypto;

namespace ROLikeMMO.Gameplay
{
    public class CasinoManager : NetworkBehaviour
    {
        public string casinoId = "Lasvegus_Main";
        public long minBet = 10;
        public long maxBet = 100000;

        [Server]
        public bool TryBet(PlayerCharacter pc, long amount)
        {
            if (amount < minBet || amount > maxBet) return false;
            var ok = EconomyManager.Instance.TryTransfer(pc.accountId, "casino_treasury", amount);
            return ok;
        }

        [Server]
        public void Payout(PlayerCharacter pc, long amount)
        {
            EconomyManager.Instance.MintTo(pc.accountId, amount);
        }
    }
}
