using Mirror;
using UnityEngine;
public class Health : NetworkBehaviour
{
    [SyncVar] public int maxHP=100;
    [SyncVar] public int hp=100;
    [SyncVar] public float defMult=1f;
    [SyncVar] public int agiBonus=0;
    [Server] public void ServerTakeDamage(int amt){ int final=Mathf.RoundToInt(amt*defMult); hp=Mathf.Max(0,hp-Mathf.Max(1,final)); }
    [Server] public void ServerTakeMagicDamage(int amt){ hp=Mathf.Max(0,hp-amt); }
    [Server] public void ServerHeal(int amt){ hp=Mathf.Min(maxHP,hp+amt); }
    [Server] public void ServerDebuffDefense(float mult,float dur){ defMult=mult; Invoke(nameof(_ResetDef), dur); }
    void _ResetDef(){ defMult=1f; }
    [Server] public void ServerBuffAgi(int bonus,float dur){ agiBonus+=bonus; Invoke(nameof(_ResetAgi), dur); }
    void _ResetAgi(){ agiBonus=0; }
}