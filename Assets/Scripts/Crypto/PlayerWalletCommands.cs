using Mirror;
using UnityEngine;
using ROLikeMMO.Crypto;
using ROLikeMMO.Gameplay;

namespace ROLikeMMO.Crypto
{
    [RequireComponent(typeof(OffchainWallet))]
    public class PlayerWalletCommands : NetworkBehaviour
    {
        OffchainWallet wallet;

        void Awake() => wallet = GetComponent<OffchainWallet>();

        [Command] public void CmdDeposit(long amount)
        {
            if (amount <= 0) return;
            wallet.Deposit(amount); // mint off-chain ให้ผู้เล่น
        }

        [Command] public void CmdWithdraw(long amount)
        {
            if (amount <= 0) return;
            wallet.Withdraw(amount); // burn off-chain
        }

        // ---- Casino (หา CasinoManager ตัวแรกในซีนปัจจุบัน) ----
        [Command] public void CmdBetSlot(long amount)
        {
            if (amount <= 0 || EconomyManager.Instance == null) return;

            var casino = GameObject.FindObjectOfType<CasinoManager>();
            if (casino == null) return;

            var pc = GetComponent<PlayerCharacter>();
            if (pc == null) return;

            if (!casino.TryBet(pc, amount)) return; // โดนหักก่อน
            var slot = GameObject.FindObjectOfType<ROLikeMMO.Gameplay.SlotMachine>();
            if (slot != null) slot.ServerSpin(pc, amount);
        }

        [Command] public void CmdBetBlackjack(long amount)
        {
            if (amount <= 0 || EconomyManager.Instance == null) return;

            var casino = GameObject.FindObjectOfType<CasinoManager>();
            if (casino == null) return;

            var pc = GetComponent<PlayerCharacter>();
            if (pc == null) return;

            if (!casino.TryBet(pc, amount)) return;
            var table = GameObject.FindObjectOfType<ROLikeMMO.Gameplay.Blackjack>();
            if (table != null) table.ServerPlayRound(pc, amount);
        }
    }
}
