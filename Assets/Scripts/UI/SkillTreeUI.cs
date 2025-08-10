using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif
public class SkillTreeUI : MonoBehaviour
{
    public SkillMasterNPC master;
#if TMP_PRESENT
    public TextMeshProUGUI title;
#else
    public Text title;
#endif
    public Button learnFirstAid, learnBash, learnHeal, learnFireBolt, learnDoubleStrafe, learnSteal, learnIncreaseAGI;
    void Start()
    {
        if (title) title.text = "Skill Tree — Novice & 1st Class";
        learnFirstAid.onClick.AddListener(()=> master.Learn("first_aid"));
        learnBash.onClick.AddListener(()=> master.Learn("bash"));
        learnHeal.onClick.AddListener(()=> master.Learn("heal"));
        learnFireBolt.onClick.AddListener(()=> master.Learn("fire_bolt"));
        learnDoubleStrafe.onClick.AddListener(()=> master.Learn("double_strafe"));
        learnSteal.onClick.AddListener(()=> master.Learn("steal"));
        learnIncreaseAGI.onClick.AddListener(()=> master.Learn("increase_agi"));
    }
}