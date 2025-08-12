using Mirror;
using UnityEngine;
using ROLikeMMO.Networking;

namespace ROLikeMMO.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class MapPortal : NetworkBehaviour
    {
        public string targetScene;
        public Vector3 targetPosition;

        void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            var pc = other.GetComponent<PlayerCharacter>();
            if (pc != null && pc.connectionToClient != null)
            {
                MyNetworkManager.Instance.ServerChangeMapForPlayer(pc.connectionToClient, targetScene, targetPosition);
            }
        }
    }
}
