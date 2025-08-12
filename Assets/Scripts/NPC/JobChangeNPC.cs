using Mirror;
using UnityEngine;
<<<<<<< HEAD

public class JobChangeNPC : NetworkBehaviour
{
    // เรียกจาก UI/Button: ใส่ชื่ออาชีพ เช่น "swordsman", "mage", ...
    public void ChangeTo(string job)
    {
        var lp = NetworkClient.localPlayer;
        if (!lp) return;

        var js = lp.GetComponent<JobSystem>();
        if (js) js.CmdChangeJob(job);
    }
}
=======
public class JobChangeNPC : NetworkBehaviour
{
    public void ChangeTo(string job)
    { var lp = NetworkClient.localPlayer; if(lp) lp.GetComponent<JobSystem>().CmdChangeJob(job); }
}
>>>>>>> 8b2444b85c97f9eb6e9b77e045fbd2cb48deee6a
