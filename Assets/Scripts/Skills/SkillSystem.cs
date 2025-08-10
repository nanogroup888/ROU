using Mirror;
using UnityEngine;
using System.Collections.Generic;
public class SkillSystem : NetworkBehaviour
{
    [System.Serializable] public class SkillLevel { public string code; public int lv; }
    public List<SkillLevel> learned = new();
    Dictionary<string, double> nextReady = new();
    public bool HasSkill(string code) => learned.Exists(s=>s.code==code && s.lv>0);
    [Command] public void CmdLearn(string code)
    { var sl=learned.Find(s=>s.code==code); if (sl==null) learned.Add(new SkillLevel{code=code,lv=1}); else if (sl.lv<10) sl.lv++; }
    [Command(requiresAuthority=false)] public void CmdUse(string code, uint targetNetId=0)
    {
        if (!HasSkill(code)) return;
        if (nextReady.TryGetValue(code, out var ready) && NetworkTime.time < ready) return;
        nextReady[code] = NetworkTime.time + GetCooldown(code);
        ExecuteSkill(code, targetNetId);
    }
    double GetCooldown(string code){ return code switch {
        "bash"=>1.0, "provoke"=>5.0, "heal"=>1.0, "fire_bolt"=>1.2, "double_strafe"=>1.2, "steal"=>5.0, "first_aid"=>1.0, "increase_agi"=>3.0, _=>1.0 };}
    [Server] void ExecuteSkill(string code, uint targetNetId)
    {
        var me = GetComponent<Health>(); var js = GetComponent<JobSystem>(); var stats = js!=null?js.GetEffectiveStats():(1,1,1,1,1,1);
        GameObject target=null; if (Mirror.NetworkServer.spawned.ContainsKey(targetNetId)) target = Mirror.NetworkServer.spawned[targetNetId].gameObject;
        if (code=="bash"){ if(target){ var th=target.GetComponent<Health>(); if(th) th.ServerTakeDamage(25+stats.STR*2);} }
        else if (code=="provoke"){ if(target){ var th=target.GetComponent<Health>(); if(th) th.ServerDebuffDefense(0.8f,10f);} }
        else if (code=="heal"){ if(target==null) target=gameObject; var th=target.GetComponent<Health>(); if(th) th.ServerHeal(40+stats.INT*3); }
        else if (code=="fire_bolt"){ if(target){ var th=target.GetComponent<Health>(); if(th) th.ServerTakeMagicDamage(30+stats.INT*4);} }
        else if (code=="double_strafe"){ if(target){ var th=target.GetComponent<Health>(); if(th){ th.ServerTakeDamage(18+stats.DEX*2); th.ServerTakeDamage(18+stats.DEX*2);} } }
        else if (code=="steal"){ if(target){ var lt=target.GetComponent<Lootable>(); if(lt) lt.ServerTrySteal(gameObject); } }
        else if (code=="first_aid"){ if(me) me.ServerHeal(20); }
        else if (code=="increase_agi"){ if(me) me.ServerBuffAgi(3,120f); }
    }
}