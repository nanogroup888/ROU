using Mirror;
using UnityEngine;

namespace ROLikeMMO.DebugTools
{
    /// <summary>
    /// On-screen diagnostics to explain why clicking doesn't attack.
    /// Attach to Player prefab.
    /// </summary>
    public class CombatChecklist : NetworkBehaviour
    {
        string[] lines;

        void LateUpdate()
        {
            if (!isLocalPlayer) return;

            var list = new System.Collections.Generic.List<string>();
            var pt = GetComponent<ROLikeMMO.Gameplay.PlayerTargeting>();
            var cam = Camera.main;

            bool isOwner = (NetworkClient.localPlayer == GetComponent<NetworkIdentity>());
            list.Add($"isLocalPlayer={isLocalPlayer}  isOwner={isOwner}");
            list.Add($"MainCamera={(cam ? cam.name : "null")}");
            list.Add($"PlayerTargeting={(pt ? "OK" : "MISSING")}");

            // mask check
            int monsterLayer = LayerMask.NameToLayer("Monster");
            if (pt)
            {
                int mask = pt.targetMask.value;
                bool hasMonster = (monsterLayer >= 0) ? ((mask & (1 << monsterLayer)) != 0 || mask == 0) : (mask == 0);
                list.Add($"Mask has 'Monster'={(hasMonster ? "YES" : "NO")}  (mask={mask})");
            }

            // any monsters around (3D only here)
            int mLayer = monsterLayer >= 0 ? (1 << monsterLayer) : ~0;
            var around = Physics.OverlapSphere(transform.position, 60f, mLayer, QueryTriggerInteraction.Collide);
            list.Add($"Monsters nearby (<=60m) = {around.Length}");

            lines = list.ToArray();
        }

        void OnGUI()
        {
            if (!isLocalPlayer || lines == null) return;
            var rect = new Rect(10, 10, 680, 22);
            foreach (var ln in lines)
            {
                GUI.Label(rect, ln);
                rect.y += 18;
            }
            GUI.Label(rect, "Tip: press 'K' to force-attack nearest monster (AttackProbe).");
        }
    }
}
