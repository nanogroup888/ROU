using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class MonsterSpawner : NetworkBehaviour
    {
        public GameObject monsterPrefab;
        public int count = 3;
        public float radius = 10f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            for (int i = 0; i < count; i++)
            {
                var pos = transform.position + Random.insideUnitSphere * radius;
                pos.y = transform.position.y;
                var m = Instantiate(monsterPrefab, pos, Quaternion.identity);
                NetworkServer.Spawn(m);
            }
        }
    }
}
