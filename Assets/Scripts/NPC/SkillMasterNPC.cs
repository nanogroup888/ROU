using Mirror;
using UnityEngine;
<<<<<<< HEAD

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
=======
public class SkillMasterNPC : NetworkBehaviour
{
    public void Learn(string code)
    { var lp = NetworkClient.localPlayer; if(lp){ var ss=lp.GetComponent<SkillSystem>(); if(ss) ss.CmdLearn(code);} }
}
>>>>>>> 8b2444b85c97f9eb6e9b77e045fbd2cb48deee6a
