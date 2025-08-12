using Mirror;
using UnityEngine;

public class SkillMasterNPC : NetworkBehaviour
{
    public void Learn(string code)
    {
        var lp = NetworkClient.localPlayer;
        if (lp)
        {
            var ss = lp.GetComponent<SkillSystem>();
            if (ss) ss.CmdLearn(code);
        }
    }
}
