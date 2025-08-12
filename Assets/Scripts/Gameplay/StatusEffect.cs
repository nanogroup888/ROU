using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public enum StatusType { None, Poison, Stun, Slow, Haste }

    public class StatusEffectController : NetworkBehaviour
    {
        class Effect
        {
            public StatusType type;
            public float endTime;
        }
        private readonly List<Effect> effects = new();

        [Server]
        public void Apply(StatusType type, float duration)
        {
            effects.Add(new Effect { type = type, endTime = Time.time + duration });
        }

        [ServerCallback]
        void Update()
        {
            float now = Time.time;
            for (int i = effects.Count - 1; i >= 0; --i)
            {
                if (effects[i].endTime <= now) effects.RemoveAt(i);
            }
        }
    }
}
