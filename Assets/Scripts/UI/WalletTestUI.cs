using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using ROLikeMMO.Crypto;

public class WalletTestUI : MonoBehaviour
{
    [Header("Assign in Inspector (or will auto-find)")]
    public InputField amountField;
    public Button depositBtn;
    public Button withdrawBtn;
    public Button slotBtn;
    public Button blackjackBtn;
    public Text balanceText;

    PlayerWalletCommands cmds;
    OffchainWallet wallet;

    void Awake()
    {
        AutoWireIfNull();
    }

    void OnEnable()
    {
        // Try auto-wire again in case UI was instantiated later
        AutoWireIfNull();
        StartCoroutine(BindWhenReady());
    }

    IEnumerator BindWhenReady()
    {
        // Wait for local player to exist
        float timeout = Time.time + 5f;
        while (NetworkClient.localPlayer == null && Time.time < timeout)
            yield return null;

        if (NetworkClient.localPlayer != null)
        {
            cmds = NetworkClient.localPlayer.GetComponent<PlayerWalletCommands>();
            wallet = NetworkClient.localPlayer.GetComponent<OffchainWallet>();
        }

        // Hook buttons safely
        if (depositBtn != null)   depositBtn.onClick.AddListener(() => Call(c => c.CmdDeposit(ReadAmount())));
        if (withdrawBtn != null)  withdrawBtn.onClick.AddListener(() => Call(c => c.CmdWithdraw(ReadAmount())));
        if (slotBtn != null)      slotBtn.onClick.AddListener(() => Call(c => c.CmdBetSlot(ReadAmount())));
        if (blackjackBtn != null) blackjackBtn.onClick.AddListener(() => Call(c => c.CmdBetBlackjack(ReadAmount())));

        if (amountField == null) Debug.LogWarning("[WalletTestUI] amountField not assigned and auto-find failed.", this);
        if (depositBtn == null) Debug.LogWarning("[WalletTestUI] depositBtn not assigned and auto-find failed.", this);
        if (withdrawBtn == null) Debug.LogWarning("[WalletTestUI] withdrawBtn not assigned and auto-find failed.", this);
        if (slotBtn == null) Debug.LogWarning("[WalletTestUI] slotBtn not assigned and auto-find failed.", this);
        if (blackjackBtn == null) Debug.LogWarning("[WalletTestUI] blackjackBtn not assigned and auto-find failed.", this);
        if (balanceText == null) Debug.LogWarning("[WalletTestUI] balanceText not assigned and auto-find failed.", this);
    }

    void Update()
    {
        if (wallet == null && NetworkClient.localPlayer != null)
        {
            wallet = NetworkClient.localPlayer.GetComponent<OffchainWallet>();
        }
        if (wallet != null && balanceText != null)
        {
            balanceText.text = $"Balance: {wallet.balance}";
        }
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
        else
            Debug.LogWarning("[WalletTestUI] Local player/cmds not ready yet.", this);
    }

    void AutoWireIfNull()
    {
        // Find in children by type if not assigned
        if (amountField == null) amountField = GetComponentInChildren<InputField>(true);
        if (balanceText == null) balanceText = GetComponentInChildren<Text>(true);

        // Try to match buttons by child text/name keywords
        if (depositBtn == null)   depositBtn = FindButtonByKeywords("deposit", "topup", "mint");
        if (withdrawBtn == null)  withdrawBtn = FindButtonByKeywords("withdraw", "burn", "cashout");
        if (slotBtn == null)      slotBtn = FindButtonByKeywords("slot", "spin");
        if (blackjackBtn == null) blackjackBtn = FindButtonByKeywords("blackjack", "bj");
    }

    Button FindButtonByKeywords(params string[] keys)
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            string n = b.name.ToLowerInvariant();
            var label = b.GetComponentInChildren<Text>(true);
            string t = label != null ? label.text.ToLowerInvariant() : "";
            foreach (var k in keys)
            {
                if (n.Contains(k) || t.Contains(k)) return b;
            }
        }
        return null;
    }
}
