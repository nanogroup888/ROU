using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ROLikeMMO.Gameplay
{
    [DisallowMultipleComponent]
    public class PlayerTargeting3D : NetworkBehaviour
    {
        public LayerMask targetMask;
        public float rayDistance = 100f;
        public bool logEvents = false;
        Camera cam;

        void Update()
        {
            if (!isLocalPlayer) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (!cam) cam = Camera.main; if (!cam) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (TryRaycastTarget(out NetworkIdentity ni))
                {
                    bool sent = false;
                    if (TryGetComponent<PlayerCombat>(out var combat))
                    {
                        combat.CmdAttackTarget(ni.netId);
                        sent = true;
                    }
                    else if (TryGetComponent<PlayerCharacter>(out var legacy))
                    {
                        var m = legacy.GetType().GetMethod("CmdAttackTarget", new System.Type[]{ typeof(uint) });
                        if (m != null) { m.Invoke(legacy, new object[]{ ni.netId }); sent = true; }
                    }

                    if (logEvents) Debug.Log(sent
                        ? $"[Targeting3D] Attack {ni.name} netId={ni.netId}"
                        : "[Targeting3D] No combat script found on Player.");
                }
                else if (logEvents) Debug.Log("[Targeting3D] No target under cursor.");
            }
        }

        bool TryRaycastTarget(out NetworkIdentity ni)
        {
            ni = null;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            int mask = targetMask.value == 0 ? ~0 : targetMask.value;
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, mask, QueryTriggerInteraction.Collide))
            {
                ni = hit.transform.GetComponentInParent<NetworkIdentity>();
                return ni != null;
            }
            return false;
        }
    }
}
