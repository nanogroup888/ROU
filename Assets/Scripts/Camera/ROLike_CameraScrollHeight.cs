// ------------------------------------------------
// File: Assets/Scripts/Camera/ROLike_CameraScrollHeight.cs  (Hotfix: add using System.Linq)
// ------------------------------------------------
using UnityEngine;
using ROLike.Common;
using System.Linq;   // <-- needed for FirstOrDefault in reflection code

namespace ROLike.CameraTools {
    public class ROLike_CameraScrollHeight : MonoBehaviour {
        public Transform cam;
        public Transform lookTarget;
        public float minHeight=8f,maxHeight=50f,scrollSpeed=5f,smooth=10f;
        public bool adjustPitchWithHeight=true; [Range(10f,85f)] public float minPitch=35f,maxPitch=65f;
        public float backPerHeight=0.9f;
        float t=0.5f,curHeight,curPitch;
        void Awake(){ if(!cam && Camera.main) cam=Camera.main.transform;
            curHeight = cam? Mathf.Max(1f,cam.localPosition.y):Mathf.Lerp(minHeight,maxHeight,t);
            t=Mathf.InverseLerp(minHeight,maxHeight,curHeight); curPitch=Mathf.Lerp(minPitch,maxPitch,t); }
        void Update(){
            float s = 0f;
            #if ENABLE_INPUT_SYSTEM
            try {
                var inputAsm = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a=>a.GetName().Name=="Unity.InputSystem");
                if (inputAsm!=null){
                    var mouseType = inputAsm.GetType("UnityEngine.InputSystem.Mouse");
                    var mouse = mouseType?.GetProperty("current", System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static)?.GetValue(null,null);
                    if (mouse!=null){
                        var scr = mouse.GetType().GetProperty("scroll")?.GetValue(mouse,null);
                        if (scr!=null){
                            var y = (float)(scr.GetType().GetProperty("y")?.GetValue(scr,null) ?? 0f);
                            s = y*0.01f;
                        }
                    }
                }
            } catch {}
            #endif
            if (Mathf.Approximately(s,0f)) s = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(s)>0.0001f) t=Mathf.Clamp01(t + s*scrollSpeed*Time.unscaledDeltaTime);

            float targetH=Mathf.Lerp(minHeight,maxHeight,t), targetP=Mathf.Lerp(minPitch,maxPitch,t);
            curHeight=Mathf.Lerp(curHeight,targetH,1f-Mathf.Exp(-smooth*Time.unscaledDeltaTime));
            curPitch =Mathf.Lerp(curPitch ,targetP,1f-Mathf.Exp(-smooth*Time.unscaledDeltaTime));

            if (cam){
                var local=cam.localPosition; local.y=curHeight; local.z=-curHeight*backPerHeight; cam.localPosition=local;
                if (adjustPitchWithHeight){ var e=cam.localEulerAngles; e.x=curPitch; cam.localEulerAngles=e; }
                if (lookTarget) cam.LookAt(lookTarget.position);
            }
        }
    }
}