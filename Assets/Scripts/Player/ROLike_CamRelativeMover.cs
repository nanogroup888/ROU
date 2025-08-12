using UnityEngine;
using System;
using System.Reflection;
using ROLike.Common;
namespace ROLike.Control {
    public class ROLike_CamRelativeMover : MonoBehaviour {
        [Header("References")]
        public Transform cameraTransform;     // leave empty → Camera.main
        public Transform headingSource;       // optional yaw source (e.g., CameraRig). If set, use this instead of camera.
        public CharacterController characterController;
        public Rigidbody rigidbodyComponent;
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float runMultiplier = 1.7f;
        public float acceleration = 12f;
        public bool faceMoveDirection = true;
        [Header("Ground (CC)")]
        public float gravity = -20f;
        public float jumpSpeed = 6f;
        public bool allowJump = false;
        [Header("Networking")]
        public bool requireLocalPlayer = true;

        Vector3 velocity;
        Component mirrorIdentity;
        PropertyInfo mirrorIsLocalProp;

        void Awake(){
            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
            if (!characterController) characterController = GetComponent<CharacterController>();
            if (!rigidbodyComponent) rigidbodyComponent = GetComponent<Rigidbody>();
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies()){
                Type t=null; try{ t=a.GetType("Mirror.NetworkIdentity"); }catch{}
                if (t!=null){ mirrorIdentity=GetComponent(t); if (mirrorIdentity!=null) mirrorIsLocalProp=t.GetProperty("isLocalPlayer"); break; }
            }
        }

        void Update(){
            if (!IsInputAllowed()) return;
            Vector2 in2 = ROLike_InputCompat.ReadMoveAxes();
            float speed = moveSpeed * (ROLike_InputCompat.IsRunHeld()? runMultiplier:1f);

            // choose heading
            Transform h = headingSource ? headingSource : cameraTransform;
            float yaw = 0f;
            if (h) yaw = h.eulerAngles.y; // pure yaw
            // convert input to world using yaw
            Vector3 localMove = new Vector3(in2.x,0f,in2.y);
            Vector3 move = Quaternion.Euler(0f,yaw,0f) * localMove;
            if (move.sqrMagnitude > 1f) move.Normalize();

            if (characterController){
                if (characterController.isGrounded){
                    velocity.y = 0f;
                    if (allowJump && ROLike_InputCompat.WasJumpPressedThisFrame()) velocity.y = jumpSpeed;
                }
                velocity.y += gravity * Time.deltaTime;
                characterController.Move(move*speed*Time.deltaTime + Vector3.up*velocity.y*Time.deltaTime);
            } else if (rigidbodyComponent){
                Vector3 targetVel = move*speed;
                Vector3 vel = rigidbodyComponent.velocity;
                Vector3 velChange = targetVel - new Vector3(vel.x,0f,vel.z);
                Vector3 accel = Vector3.ClampMagnitude(velChange, acceleration);
                rigidbodyComponent.AddForce(accel, ForceMode.VelocityChange);
            } else {
                transform.position += move*speed*Time.deltaTime;
            }

            if (faceMoveDirection && move.sqrMagnitude>0.0001f){
                var rot = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 20f*Time.deltaTime);
            }
        }

        bool IsInputAllowed(){
            if (!requireLocalPlayer || mirrorIdentity==null || mirrorIsLocalProp==null) return true;
            try{ var v=mirrorIsLocalProp.GetValue(mirrorIdentity,null); if (v is bool b) return b; }catch{}
            return true;
        }
    }
}