using UnityEngine; using UnityEngine.UI; using Mirror;
public class HotbarUI : MonoBehaviour
{
    public Button slot1,slot2,slot3,slot4,slot5,slot6; SkillSystem ss;
    void Start(){ var lp=NetworkClient.localPlayer; if(lp) ss=lp.GetComponent<SkillSystem>();
        slot1.onClick.AddListener(()=> Use("bash"));
        slot2.onClick.AddListener(()=> Use("provoke"));
        slot3.onClick.AddListener(()=> Use("heal"));
        slot4.onClick.AddListener(()=> Use("fire_bolt"));
        slot5.onClick.AddListener(()=> Use("double_strafe"));
        slot6.onClick.AddListener(()=> Use("first_aid")); }
    void Use(string code){ if(ss) ss.CmdUse(code, 0); }
}