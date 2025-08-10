using Mirror;
using UnityEngine;
public class Lootable : NetworkBehaviour
{
    public int zenyOnSteal = 5;
    [Server] public void ServerTrySteal(GameObject who)
    { if (Random.value < 0.25f){ var cur=who.GetComponent<PlayerCurrency>(); if(cur) cur.zenyOffchain += zenyOnSteal; } }
}