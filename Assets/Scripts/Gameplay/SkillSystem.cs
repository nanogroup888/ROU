using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    [Serializable]
    public class SkillDef
    {
        public uint id;
        public string name;
        public int spCost;
        public float cooldown;
        public string effect; // e.g. "Damage:30" or "Heal:50"
    }

    public class SkillSystem : NetworkBehaviour
    {
        Dictionary<uint, float> cd = new();

        [Server]
        public bool CanCast(uint skillId)
        {
            return !cd.ContainsKey(skillId) || Time.time >= cd[skillId];
        }

        [Server]
        public void Cast(PlayerCharacter caster, uint skillId, Vector3 targetPoint)
        {
            if (!CanCast(skillId)) return;
            cd[skillId] = Time.time + 1f; // placeholder cooldown
            // TODO: apply effect
        }
    }
}
