using System.Collections;
using Mirror;
using UnityEngine;

namespace ROLikeMMO.Crypto
{
    [DisallowMultipleComponent]
    public class OffchainWallet : NetworkBehaviour
    {
        [SyncVar] public long balance;

        public override void OnStartServer()
        {
            base.OnStartServer();
            // Delay init until EconomyManager is available and PlayerCharacter is ready
            StartCoroutine(InitWhenReady());
        }

        IEnumerator InitWhenReady()
        {
            // Wait for PlayerCharacter component
            var tries = 0;
            var pc = GetComponent<ROLikeMMO.Gameplay.PlayerCharacter>();
            while (pc == null && tries < 120) // ~2 seconds @ 60fps
            {
                yield return null;
                tries++;
                pc = GetComponent<ROLikeMMO.Gameplay.PlayerCharacter>();
            }
            if (pc == null) yield break;

            // Wait for EconomyManager.Instance
            tries = 0;
            while (EconomyManager.Instance == null && tries < 300) // ~5 seconds
            {
                yield return null;
                tries++;
            }
            if (EconomyManager.Instance == null) yield break;

            // Initialize balance safely
            balance = EconomyManager.Instance.GetBalance(pc.accountId);
        }

        [Server]
        public bool Deposit(long amount)
        {
            var pc = GetComponent<ROLikeMMO.Gameplay.PlayerCharacter>();
            if (EconomyManager.Instance == null || pc == null) return false;
            bool ok = EconomyManager.Instance.MintTo(pc.accountId, amount);
            if (ok) balance = EconomyManager.Instance.GetBalance(pc.accountId);
            return ok;
        }

        [Server]
        public bool Withdraw(long amount)
        {
            var pc = GetComponent<ROLikeMMO.Gameplay.PlayerCharacter>();
            if (EconomyManager.Instance == null || pc == null) return false;
            bool ok = EconomyManager.Instance.BurnFrom(pc.accountId, amount);
            if (ok) balance = EconomyManager.Instance.GetBalance(pc.accountId);
            return ok;
        }
    }
}
