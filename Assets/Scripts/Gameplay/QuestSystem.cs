using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class QuestSystem : NetworkBehaviour
    {
        // Placeholder for quest tracking
        private readonly HashSet<string> completed = new();

        [Server]
        public void Complete(string questId)
        {
            if (completed.Add(questId))
            {
                // reward etc.
            }
        }
    }
}
