using Mirror;
using UnityEngine;
public class PlayerCurrency : NetworkBehaviour
{
    [SyncVar] public int zenyOffchain = 100;
    [TargetRpc] public void TargetAddZeny(NetworkConnection target, int amount, string reason)
    { zenyOffchain += amount; Debug.Log($"+{amount} ZENY ({reason})"); }
}