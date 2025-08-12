using System.Collections;
using Mirror;
using UnityEngine;

namespace ROLikeMMO.Crypto
{
    public class OnChainBridge : NetworkBehaviour
    {
        [Header("HTTP Endpoint (placeholder)")]
        public string depositApi = "https://your-chain.example.com/deposit";
        public string withdrawApi = "https://your-chain.example.com/withdraw";

        [Server]
        public void BeginDeposit(string accountId, long amount, string targetAddress)
        {
            // TODO: call external API, verify tx, then credit Off-chain
            StartCoroutine(SimulateDeposit(accountId, amount));
        }

        [Server]
        public void BeginWithdraw(string accountId, long amount, string targetAddress)
        {
            // TODO: burn off-chain then call external API to mint on-chain
            var econ = EconomyManager.Instance;
            if (econ.BurnFrom(accountId, amount))
            {
                // simulate success
                // if failed, refund by MintTo
            }
        }

        IEnumerator SimulateDeposit(string accountId, long amount)
        {
            yield return new WaitForSeconds(2f);
            EconomyManager.Instance.MintTo(accountId, amount);
        }
    }
}
