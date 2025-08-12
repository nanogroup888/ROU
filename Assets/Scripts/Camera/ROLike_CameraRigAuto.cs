// ------------------------------------------------
// File: Assets/Scripts/Camera/ROLike_CameraRigAuto.cs
// Attach to CameraRig. Auto-finds Player/localPlayer and wires targets.
//  - Sets OrbitFollow.target and ScrollHeight.lookTarget
//  - Optionally assigns mover.headingSource/cameraTransform
// ------------------------------------------------
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using ROLike.CameraTools;

namespace ROLike.CameraTools {
    public class ROLike_CameraRigAuto : MonoBehaviour {
        public Transform explicitPlayer;      // set manually if you want
        public bool preferMirrorLocalPlayer = true;
        public bool assignHeadingToMover = true;

        void Start(){
            var player = explicitPlayer ? explicitPlayer : FindPlayer();
            if (player == null){
                Debug.LogWarning("[ROLike_CameraRigAuto] Player not found. Set explicitPlayer.");
                return;
            }
            // Wire OrbitFollow
            var orbit = GetComponent<ROLike_CameraOrbitFollow>();
            if (orbit){ orbit.target = player; }
            // Wire ScrollHeight
            var scroll = GetComponent<ROLike_CameraScrollHeight>();
            if (scroll){ scroll.lookTarget = player; if (!scroll.cam && Camera.main) scroll.cam = Camera.main.transform; }
            // Assign to mover (camera-relative)
            if (assignHeadingToMover){
                var mover = player.GetComponent("ROLike.Control.ROLike_CamRelativeMover");
                if (mover != null){
                    var t = mover.GetType();
                    try {
                        var cam = Camera.main ? Camera.main.transform : null;
                        var fCam = t.GetField("cameraTransform"); var fHead = t.GetField("headingSource");
                        if (fCam != null) fCam.SetValue(mover, cam);
                        if (fHead!= null) fHead.SetValue(mover, this.transform);
                    } catch {}
                }
            }
            Debug.Log($"[ROLike_CameraRigAuto] Wired camera rig to player: {player.name}");
        }

        Transform FindPlayer(){
            // 1) Mirror local player
            if (preferMirrorLocalPlayer){
                try {
                    var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a=>a.GetName().Name=="Mirror");
                    var niType = asm?.GetType("Mirror.NetworkIdentity");
                    if (niType != null){
                        var all = GameObject.FindObjectsOfType(typeof(Transform), true).Cast<Transform>();
                        foreach (var tr in all){
                            var id = tr.GetComponent(niType);
                            if (id != null){
                                var prop = niType.GetProperty("isLocalPlayer");
                                if (prop != null && prop.GetValue(id, null) is bool b && b) return tr;
                            }
                        }
                    }
                } catch {}
            }
            // 2) Tag = Player
            var byTag = GameObject.FindGameObjectWithTag("Player");
            if (byTag) return byTag.transform;
            // 3) Name contains "Player"
            var allTs = GameObject.FindObjectsOfType<Transform>(true);
            var named = allTs.FirstOrDefault(t=>t.name.ToLower().Contains("player"));
            if (named) return named;
            // 4) Has our mover
            var moverType = Type.GetType("ROLike.Control.ROLike_CamRelativeMover");
            if (moverType != null){
                foreach (var tr in allTs){
                    if (tr.GetComponent(moverType) != null) return tr;
                }
            }
            return null;
        }
    }
}