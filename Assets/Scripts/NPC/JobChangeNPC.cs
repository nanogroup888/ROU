using Mirror;
using UnityEngine;
public class JobChangeNPC : NetworkBehaviour
{
    public void ChangeTo(string job)
    { var lp = NetworkClient.localPlayer; if(lp) lp.GetComponent<JobSystem>().CmdChangeJob(job); }
}