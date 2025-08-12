using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace ROLikeMMO.Gameplay
{
    /// <summary>
    /// Optional NavMesh-based monster AI for 3D worlds.
    /// Requires baked NavMesh and a NavMeshAgent on the root.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterAI_Nav : NetworkBehaviour
    {
        public int hp = 40;
        public float roamRadius = 8f;
        public float changeEvery = 3f;

        NavMeshAgent agent;
        Health health;
        float timer;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>() ?? gameObject.AddComponent<Health>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            health.SetMax(hp, true);
            timer = changeEvery;
            PickNewDestination();
        }

        void Update()
        {
            if (!isServer) return;

            timer -= Time.deltaTime;
            if (timer <= 0f || (!agent.pathPending && agent.remainingDistance < 0.3f))
            {
                PickNewDestination();
                timer = changeEvery;
            }

            // Drive animator speed if available
            float speed = agent.velocity.magnitude;
            RpcSetSpeed(speed);

            if (health.current <= 0)
            {
                RpcDie();
                NetworkServer.Destroy(gameObject);
            }
        }

        void PickNewDestination()
        {
            Vector3 random = transform.position + new Vector3(Random.Range(-roamRadius, roamRadius), 0, Random.Range(-roamRadius, roamRadius));
            if (NavMesh.SamplePosition(random, out var hit, roamRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        [Server]
        public void ServerTakeDamage(int dmg, Vector3 hitPos)
        {
            health.Damage(dmg);
            RpcHit();
            ROLikeMMO.UI.DamagePopupManager.Instance?.ShowDamageWorld(hitPos, dmg, false);
        }

        [ClientRpc] public void RpcSetSpeed(float s) { var a = GetComponentInChildren<Animator>(); if (a) a.SetFloat("Speed", s); }
        [ClientRpc] public void RpcHit() { var a = GetComponentInChildren<Animator>(); if (a) a.SetTrigger("Hit"); }
        [ClientRpc] public void RpcDie() { var a = GetComponentInChildren<Animator>(); if (a) a.SetTrigger("Die"); }
    }
}
