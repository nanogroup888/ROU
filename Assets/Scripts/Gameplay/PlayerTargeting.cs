using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    public class PlayerTargeting : NetworkBehaviour
    {
        [Header("Targeting")]
        public LayerMask targetMask;
        public float selectMaxDistance = 50f;
        public float attackRange = 3.5f;

        public GameObject selectionIndicatorPrefab;
        GameObject indicatorInstance;

        Camera cam;
        NetworkIdentity currentTarget;

        void Start()
        {
            if (!isLocalPlayer) return;
            cam = Camera.main;
            if (cam == null) Debug.LogWarning("[Targeting] MainCamera not found");
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            // คลิกซ้าย = เลือก
            if (Input.GetMouseButtonDown(0))
            {
                if (TrySelectUnderMouse(out bool same))
                {
                    Debug.Log($"[Targeting] Selected: {currentTarget.name}");
                }
                else
                {
                    Debug.Log("[Targeting] Raycast miss");
                }
            }

            // คลิกขวา = โจมตีเป้าปัจจุบัน
            if (Input.GetMouseButtonDown(1) && currentTarget != null)
            {
                TryAttackCurrent();
            }

            // ปุ่ม T = โจมตีเป้าปัจจุบัน (ทางลัดดีบัก)
            if (Input.GetKeyDown(KeyCode.T) && currentTarget != null)
            {
                TryAttackCurrent();
            }

            if (indicatorInstance && currentTarget)
            {
                var p = currentTarget.transform.position;
                indicatorInstance.transform.position = new Vector3(p.x, p.y + 0.05f, p.z);
            }
        }

        void TryAttackCurrent()
        {
            float d = Vector3.Distance(transform.position, currentTarget.transform.position);
            Debug.Log($"[Targeting] Attack try {currentTarget.name}, dist={d:F2}, range={attackRange}");
            if (d <= attackRange + 0.2f)
            {
                CmdRequestAttack(currentTarget.netId);
            }
            else
            {
                Debug.Log("[Targeting] Too far. Move closer.");
            }
        }

        bool TrySelectUnderMouse(out bool sameTarget)
        {
            sameTarget = false;
            if (cam == null) cam = Camera.main;

            int mask = targetMask.value == 0 ? ~0 : targetMask.value; // ถ้าไม่ได้ตั้ง ให้ชนทุกเลเยอร์
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, selectMaxDistance, mask))
            {
                var ni = hit.collider.GetComponentInParent<NetworkIdentity>();
                if (ni == null) return false;

                if (currentTarget == ni)
                {
                    sameTarget = true;
                    return true;
                }

                currentTarget = ni;
                if (selectionIndicatorPrefab != null)
                {
                    if (indicatorInstance == null) indicatorInstance = Instantiate(selectionIndicatorPrefab);
                    indicatorInstance.transform.SetParent(null);
                    indicatorInstance.transform.position = ni.transform.position + Vector3.up * 0.05f;
                }
                return true;
            }
            return false;
        }

        [Command]
        void CmdRequestAttack(uint targetNetId)
        {
            Debug.Log($"[Targeting/Cmd] Attack request netId={targetNetId}");
            var pc = GetComponent<PlayerCharacter>();
            if (pc != null) pc.CmdAttackTarget(targetNetId);
        }
    }
}
