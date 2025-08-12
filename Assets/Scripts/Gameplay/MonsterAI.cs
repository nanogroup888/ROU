using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    // Core logic only. RPC methods are implemented in the partial file:
    // "MonsterAI.AnimatorAdapterHook.cs"
    public partial class MonsterAI : NetworkBehaviour
    {
        [Header("Stats (legacy)")]
        public int hp = 30;
        public float wanderRadius = 5f;
        public float moveSpeed = 1.5f;

        Vector3 origin;
        Vector3 target;
        float changeTargetTimer;
        Health health;

        void Awake()
        {
            health = GetComponent<Health>();
            if (health == null) health = gameObject.AddComponent<Health>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            origin = transform.position;
            health.SetMax(hp, true);
            PickNewTarget();
        }

        void PickNewTarget()
        {
            target = origin + new Vector3(Random.Range(-wanderRadius, wanderRadius), 0, Random.Range(-wanderRadius, wanderRadius));
            changeTargetTimer = Random.Range(2f, 4f);
        }

        void Update()
        {
            if (!isServer) return;

            changeTargetTimer -= Time.deltaTime;
            var to = target - transform.position; to.y = 0;
            float dist = to.magnitude;

            float speed = 0f;
            if (dist > 0.1f)
            {
                speed = moveSpeed;
                var dir = to.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
            }

            // RPCs are defined in the other partial file
            RpcSetSpeed(speed);

            if (changeTargetTimer <= 0f || dist < 0.2f)
                PickNewTarget();

            if (health.current <= 0)
            {
                RpcDie();
                Mirror.NetworkServer.Destroy(gameObject);
            }
        }

        [Server]
        public void ServerTakeDamage(int dmg, Vector3 hitPos)
        {
            health.Damage(dmg);
            RpcHit();
            ROLikeMMO.UI.DamagePopupManager.Instance?.ShowDamageWorld(hitPos, dmg, false);
        }

        // NOTE: Do NOT add Rpc method bodies here. They live in MonsterAI.AnimatorAdapterHook.cs
        // Having them in both files will cause CS0111 duplicate member errors.
    }
}
