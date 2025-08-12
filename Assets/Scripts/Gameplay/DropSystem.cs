using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class DropSystem : NetworkBehaviour
    {
        [Server]
        public void Drop(uint itemId, int qty, Vector3 where)
        {
            // Placeholder: spawn a pickup object (not implemented)
        }
    }
}
