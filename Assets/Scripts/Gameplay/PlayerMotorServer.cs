using Mirror;
using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotorServer : NetworkBehaviour
    {
        public float walkSpeed = 4f;
        public float runSpeed = 6f;
        public float rotateSpeed = 720f;
        public float gravity = -20f;

        CharacterController cc;
        Vector3 serverMoveDir;
        bool serverRun;
        float verticalVel;

        void Awake() { cc = GetComponent<CharacterController>(); }

        void Update()
        {
            if (isLocalPlayer) ReadClientInputAndSend();
            if (isServer) ServerMove();
        }

        void ReadClientInputAndSend()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            bool run = Input.GetKey(KeyCode.LeftShift);

            Camera cam = Camera.main;
            Vector3 fwd = cam ? cam.transform.forward : Vector3.forward;
            Vector3 right = cam ? cam.transform.right : Vector3.right;
            fwd.y = 0; right.y = 0;
            fwd.Normalize(); right.Normalize();

            Vector3 worldDir = (fwd * v + right * h);
            if (worldDir.sqrMagnitude > 1f) worldDir.Normalize();

            CmdSetMove(worldDir, run);
        }

        [Command]
        void CmdSetMove(Vector3 worldDir, bool run)
        {
            serverMoveDir = worldDir;
            serverRun = run;
        }

        void ServerMove()
        {
            float speed = serverRun ? runSpeed : walkSpeed;

            Vector3 planar = new Vector3(serverMoveDir.x, 0, serverMoveDir.z);
            if (planar.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(planar, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }

            if (cc.isGrounded) verticalVel = -0.5f;
            else verticalVel += gravity * Time.deltaTime;

            Vector3 velocity = planar * speed + Vector3.up * verticalVel;
            cc.Move(velocity * Time.deltaTime);

            var anim = GetComponentInChildren<Animator>();
            if (anim) anim.SetFloat("Speed", planar.magnitude * speed);
        }
    }
}
