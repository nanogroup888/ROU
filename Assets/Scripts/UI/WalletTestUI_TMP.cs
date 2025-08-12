using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using ROLikeMMO.Crypto;

public class WalletTestUI_TMP : MonoBehaviour
{
    [Header("Assign in Inspector (TMP version)")]
    public TMP_InputField amountField;
    public Button depositBtn;
    public Button withdrawBtn;
    public Button slotBtn;
    public Button blackjackBtn;
    public TMP_Text balanceText;

    PlayerWalletCommands cmds;
    OffchainWallet wallet;

    void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    IEnumerator BindWhenReady()
    {
        // รอจนกว่าจะมี local player
        float timeout = Time.time + 5f;
        while (NetworkClient.localPlayer == null && Time.time < timeout)
            yield return null;

        if (NetworkClient.localPlayer != null)
        {
            cmds = NetworkClient.localPlayer.GetComponent<PlayerWalletCommands>();
            wallet = NetworkClient.localPlayer.GetComponent<OffchainWallet>();
        }

        // bind ปุ่มแบบปลอดภัย
        if (depositBtn != null)   depositBtn.onClick.AddListener(() => Call(c => c.CmdDeposit(ReadAmount())));
        if (withdrawBtn != null)  withdrawBtn.onClick.AddListener(() => Call(c => c.CmdWithdraw(ReadAmount())));
        if (slotBtn != null)      slotBtn.onClick.AddListener(() => Call(c => c.CmdBetSlot(ReadAmount())));
        if (blackjackBtn != null) blackjackBtn.onClick.AddListener(() => Call(c => c.CmdBetBlackjack(ReadAmount())));
    }

    void Update()
    {
        if (wallet == null && NetworkClient.localPlayer != null)
            wallet = NetworkClient.localPlayer.GetComponent<OffchainWallet>();

        if (wallet != null && balanceText != null)
            balanceText.text = $"Balance: {wallet.balance}";
    }

    long ReadAmount()
    {
        if (amountField != null && long.TryParse(amountField.text, out var v)) return v;
        return 0;
    }

    void Call(System.Action<PlayerWalletCommands> act)
    {
        if (cmds == null && NetworkClient.localPlayer != null)
            cmds = NetworkClient.localPlayer.GetComponent<PlayerWalletCommands>();

        if (cmds != null && NetworkClient.active)
            act(cmds);
    }
}
