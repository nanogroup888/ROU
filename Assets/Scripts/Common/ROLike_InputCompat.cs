// ------------------------------------------------
// File: Assets/Scripts/Common/ROLike_InputCompat.cs
// Old/New input compatible helpers (reflection for New).
// ------------------------------------------------
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
namespace ROLike.Common {
    public static class ROLike_InputCompat {
        public static Vector2 ReadMoveAxes(){
            try {
                var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a=>a.GetName().Name=="Unity.InputSystem");
                if (asm != null){
                    var kbType = asm.GetType("UnityEngine.InputSystem.Keyboard");
                    var kb = kbType?.GetProperty("current", BindingFlags.Public|BindingFlags.Static)?.GetValue(null,null);
                    if (kb != null){
                        float x = Is(kb,"dKey")||Is(kb,"rightArrowKey")?1f:0f; x -= (Is(kb,"aKey")||Is(kb,"leftArrowKey"))?1f:0f;
                        float y = Is(kb,"wKey")||Is(kb,"upArrowKey")?1f:0f;    y -= (Is(kb,"sKey")||Is(kb,"downArrowKey"))?1f:0f;
                        var v=new Vector2(x,y); if (v.sqrMagnitude>1f) v.Normalize(); return v;
                    }
                }
            } catch {}
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        public static bool IsRunHeld(){
            try{
                var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a=>a.GetName().Name=="Unity.InputSystem");
                if (asm!=null){
                    var kbType = asm.GetType("UnityEngine.InputSystem.Keyboard");
                    var kb = kbType?.GetProperty("current", BindingFlags.Public|BindingFlags.Static)?.GetValue(null,null);
                    if (kb!=null) return Is(kb,"leftShiftKey")||Is(kb,"rightShiftKey");
                }
            }catch{}
            return Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);
        }
        public static bool WasJumpPressedThisFrame(){
            try{
                var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a=>a.GetName().Name=="Unity.InputSystem");
                if (asm!=null){
                    var kbType = asm.GetType("UnityEngine.InputSystem.Keyboard");
                    var kb = kbType?.GetProperty("current", BindingFlags.Public|BindingFlags.Static)?.GetValue(null,null);
                    if (kb!=null){
                        var key = kb.GetType().GetProperty("spaceKey")?.GetValue(kb,null);
                        var prop = key?.GetType().GetProperty("wasPressedThisFrame");
                        return (bool)(prop?.GetValue(key,null) ?? false);
                    }
                }
            }catch{}
            return Input.GetKeyDown(KeyCode.Space);
        }
        static bool Is(object kb,string prop){ var k=kb.GetType().GetProperty(prop)?.GetValue(kb,null); var p=k?.GetType().GetProperty("isPressed"); return (bool)(p?.GetValue(k,null)??false); }
    }
}