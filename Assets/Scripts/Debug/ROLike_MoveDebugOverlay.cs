using UnityEngine;
using ROLike.Common;
namespace ROLike.Debugging {
    public class ROLike_MoveDebugOverlay : MonoBehaviour {
        public Transform cameraTransform;
        public Transform headingSource;
        void OnGUI(){
            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
            var axes = ROLike_InputCompat.ReadMoveAxes();
            Transform h = headingSource ? headingSource : cameraTransform;
            float yaw = h ? h.eulerAngles.y : 0f;
            GUILayout.BeginArea(new Rect(10,10,360,120), GUI.skin.box);
            GUILayout.Label($"Camera used: {(cameraTransform?cameraTransform.name:"(none)")}, yaw={yaw:F1}");
            GUILayout.Label($"Heading source: {(headingSource?headingSource.name:"(camera)")}");
            GUILayout.Label($"Move axes: {axes}  RunHeld={ROLike_InputCompat.IsRunHeld()}  JumpDown={ROLike_InputCompat.WasJumpPressedThisFrame()}");
            GUILayout.EndArea();
        }
    }
}