using Mirror;
using UnityEngine;
using System.Collections.Generic;
<<<<<<< HEAD

public class SkillSystem : NetworkBehaviour
{
    [System.Serializable]
    public class SkillLevel
    {
        public string code;
        public int lv;
    }

    // สกิลที่เรียนแล้ว
    public List<SkillLevel> learned = new();

    // คูลดาวน์ต่อสกิล (timestamp ของ NetworkTime.time)
    private readonly Dictionary<string, double> nextReady = new();

    public bool HasSkill(string code) => learned.Exists(s => s.code == code && s.lv > 0);

    // ===== Client API =====
    // Helper ที่ฝั่ง client เรียกง่าย ๆ แล้วค่อยยิง Command ไปหาเซิร์ฟเวอร์
    public void Use(string code, uint targetNetId = 0)
    {
        CmdUse(code, targetNetId);
    }

    // เรียนสกิล
    [Command]
    public void CmdLearn(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;

        var sl = learned.Find(s => s.code == code);
        if (sl == null)
            learned.Add(new SkillLevel { code = code, lv = 1 });
        else if (sl.lv < 10)
            sl.lv++;
    }

    // ใช้สกิล (ห้ามมี optional parameter บน Command)
    [Command(requiresAuthority = false)]
    public void CmdUse(string code, uint targetNetId)
    {
        if (!HasSkill(code)) return;

        if (nextReady.TryGetValue(code, out var ready) && NetworkTime.time < ready)
            return;

        nextReady[code] = NetworkTime.time + GetCooldown(code);
        ExecuteSkill(code, targetNetId);
    }

    // ===== Server side =====
    double GetCooldown(string code) => code switch
    {
        "bash"            => 1.0,
        "provoke"         => 5.0,
        "heal"            => 1.0,
        "fire_bolt"       => 1.2,
        "double_strafe"   => 1.2,
        "steal"           => 5.0,
        "first_aid"       => 1.0,
        "increase_agi"    => 3.0,
        _                 => 1.0
    };

    [Server]
    void ExecuteSkill(string code, uint targetNetId)
    {
        var me = GetComponent<Health>();
        var js = GetComponent<JobSystem>();

        // ดึงค่าสเตตัสแบบปลอดภัย (INTL = Intelligence)
        int STR = 1, AGI = 1, VIT = 1, INTL = 1, DEX = 1, LUK = 1;
        if (js != null)
        {
            var s = js.GetEffectiveStats();
            STR = s.STR; AGI = s.AGI; VIT = s.VIT; INTL = s.INTL; DEX = s.DEX; LUK = s.LUK;
        }

        GameObject target = null;
        if (targetNetId != 0 && NetworkServer.spawned.TryGetValue(targetNetId, out var ni))
            target = ni.gameObject;

        // ===== ตัวอย่างสกิลพื้นฐาน =====
        if (code == "bash")
        {
            if (target)
            {
                var th = target.GetComponent<Health>();
                if (th) th.ServerTakeDamage(25 + STR * 2);
            }
        }
        else if (code == "provoke")
        {
            if (target)
            {
                var th = target.GetComponent<Health>();
                if (th) th.ServerDebuffDefense(0.8f, 10f);
            }
        }
        else if (code == "heal")
        {
            if (!target) target = gameObject;
            var th = target.GetComponent<Health>();
            if (th) th.ServerHeal(40 + INTL * 3);
        }
        else if (code == "fire_bolt")
        {
            if (target)
            {
                var th = target.GetComponent<Health>();
                if (th) th.ServerTakeMagicDamage(30 + INTL * 4);
            }
        }
        else if (code == "double_strafe")
        {
            if (target)
            {
                var th = target.GetComponent<Health>();
                if (th)
                {
                    th.ServerTakeDamage(18 + DEX * 2);
                    th.ServerTakeDamage(18 + DEX * 2);
                }
            }
        }
        else if (code == "steal")
        {
            if (target)
            {
                var lt = target.GetComponent<Lootable>();
                if (lt) lt.ServerTrySteal(gameObject);
            }
        }
        else if (code == "first_aid")
        {
            if (me) me.ServerHeal(20);
        }
        else if (code == "increase_agi")
        {
            if (me) me.ServerBuffAgi(3, 120f);
        }
    }
}
=======
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
>>>>>>> 8b2444b85c97f9eb6e9b77e045fbd2cb48deee6a
