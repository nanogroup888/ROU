using Mirror;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable] public class JobStatBonus { public int str, agi, vit, intl, dex, luk; }

public class JobSystem : NetworkBehaviour
{
    [SyncVar] public string jobCode = "novice";

    public int baseSTR = 1, baseAGI = 1, baseVIT = 1, baseINT = 1, baseDEX = 1, baseLUK = 1;

    // ใช้ key เป็นชื่ออาชีพ
    readonly Dictionary<string, JobStatBonus> jobBonuses = new()
    {
        {"novice",    new JobStatBonus{str=0, agi=0, vit=0, intl=0, dex=0, luk=0}},
        {"swordsman", new JobStatBonus{str=2, agi=0, vit=2, intl=0, dex=1, luk=0}},
        {"mage",      new JobStatBonus{str=0, agi=0, vit=0, intl=3, dex=1, luk=0}},
        {"archer",    new JobStatBonus{str=0, agi=1, vit=0, intl=0, dex=3, luk=0}},
        {"thief",     new JobStatBonus{str=1, agi=3, vit=0, intl=0, dex=1, luk=0}},
        {"acolyte",   new JobStatBonus{str=0, agi=0, vit=1, intl=2, dex=1, luk=0}},
        {"merchant",  new JobStatBonus{str=1, agi=0, vit=1, intl=0, dex=1, luk=1}},
    };

    // ใช้ INTL เพื่อไม่ชนกับคำว่า int
    public (int STR,int AGI,int VIT,int INTL,int DEX,int LUK) GetEffectiveStats()
    {
        var b = jobBonuses.TryGetValue(jobCode, out var bonus) ? bonus : new JobStatBonus();
        return (
            baseSTR + b.str,
            baseAGI + b.agi,
            baseVIT + b.vit,
            baseINT + b.intl,
            baseDEX + b.dex,
            baseLUK + b.luk
        );
    }

    // เรียกจาก Client เพื่อสั่งเปลี่ยนอาชีพบน Server
    [Command]
    public void CmdChangeJob(string toJob)
    {
        if (string.IsNullOrWhiteSpace(toJob)) return;
        toJob = toJob.ToLowerInvariant();
        if (!jobBonuses.ContainsKey(toJob)) return;

        // ตัวอย่างเงื่อนไข: อนุญาตให้เปลี่ยนจาก novice เท่านั้น
        if (jobCode == "novice" || toJob == "novice")
            jobCode = toJob;
    }
}
