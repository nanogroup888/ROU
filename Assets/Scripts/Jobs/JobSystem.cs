using Mirror;
using UnityEngine;
using System.Collections.Generic;
[System.Serializable] public class JobStatBonus { public int str,agi,vit,intl,dex,luk; }
public class JobSystem : NetworkBehaviour
{
    [SyncVar] public string jobCode = "novice";
    public Dictionary<string, JobStatBonus> jobBonuses = new Dictionary<string, JobStatBonus>{
        {"novice", new JobStatBonus()},
        {"swordsman", new JobStatBonus{str=2,vit=2,dex=1}},
        {"mage", new JobStatBonus{intl=3,dex=1}},
        {"archer", new JobStatBonus{agi=1,dex=3}},
        {"thief", new JobStatBonus{str=1,agi=3,dex=1}},
        {"acolyte", new JobStatBonus{intl=2,vit=1,dex=1}},
        {"merchant", new JobStatBonus{str=1,vit=1,dex=1,luk=1}},
    };
    public int baseSTR=1,baseAGI=1,baseVIT=1,baseINT=1,baseDEX=1,baseLUK=1;
    public (int STR,int AGI,int VIT,int INT,int DEX,int LUK) GetEffectiveStats()
    {
        var b = jobBonuses.ContainsKey(jobCode) ? jobBonuses[jobCode] : new JobStatBonus();
        return (baseSTR+b.str, baseAGI+b.agi, baseVIT+b.vit, baseINT+b.intl, baseDEX+b.dex, baseLUK+b.luk);
    }
    [Command] public void CmdChangeJob(string toJob)
    { if (jobCode=="novice") if (jobBonuses.ContainsKey(toJob)) jobCode = toJob; }
}