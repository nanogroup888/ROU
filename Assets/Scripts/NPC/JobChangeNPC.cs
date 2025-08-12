using Mirror;
using UnityEngine;

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
