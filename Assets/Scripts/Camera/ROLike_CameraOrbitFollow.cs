// ------------------------------------------------
// File: Assets/Scripts/Camera/ROLike_CameraOrbitFollow.cs
// Attach to CameraRig (parent of Main Camera).
// ------------------------------------------------
using UnityEngine;
using ROLike.Common;

namespace ROLike.CameraTools {
    public class ROLike_CameraOrbitFollow : MonoBehaviour {
        [Header("Follow")]
        public Transform target;
        public float followSmoothTime = 0.06f;
        Vector3 vel;

        [Header("Orbit (Yaw)")]
        public bool rmbToRotate = true;
        public float mouseYawSpeed = 0.25f; // deg per pixel
        public float keyYawSpeed = 120f;    // deg per sec
        public float yaw;

        void Start(){ yaw = transform.eulerAngles.y; }

        void LateUpdate(){
            if (target){
                transform.position = Vector3.SmoothDamp(transform.position, target.position, ref vel, followSmoothTime);
            }
            bool doRotate = !rmbToRotate || ROLike_MouseCompat.RightHeld();
            if (doRotate){
                var d = ROLike_MouseCompat.ReadDelta();
                yaw += d.x * mouseYawSpeed;
            }
            if (Input.GetKey(KeyCode.Q)) yaw -= keyYawSpeed * Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.E)) yaw += keyYawSpeed * Time.unscaledDeltaTime;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}