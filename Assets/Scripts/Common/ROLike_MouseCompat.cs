// ------------------------------------------------
// File: Assets/Scripts/Common/ROLike_MouseCompat.cs
// Read mouse delta & RMB for Old/New Input System (reflection; no hard dep).
// ------------------------------------------------
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;

namespace ROLike.Common {
    public static class ROLike_MouseCompat {
        public static Vector2 ReadDelta(){
            try {
                var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name=="Unity.InputSystem");
                if (asm != null){
                    var mouseType = asm.GetType("UnityEngine.InputSystem.Mouse");
                    var mouse = mouseType?.GetProperty("current", BindingFlags.Public|BindingFlags.Static)?.GetValue(null,null);
                    if (mouse != null){
                        var delta = mouse.GetType().GetProperty("delta")?.GetValue(mouse,null);
                        if (delta != null){
                            var x = (float)(delta.GetType().GetProperty("x")?.GetValue(delta,null) ?? 0f);
                            var y = (float)(delta.GetType().GetProperty("y")?.GetValue(delta,null) ?? 0f);
                            return new Vector2(x,y);
                        }
                    }
                }
            } catch {}
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }
        public static bool RightHeld(){
            try {
                var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name=="Unity.InputSystem");
                if (asm != null){
                    var mouseType = asm.GetType("UnityEngine.InputSystem.Mouse");
                    var mouse = mouseType?.GetProperty("current", BindingFlags.Public|BindingFlags.Static)?.GetValue(null,null);
                    if (mouse != null){
                        var btn = mouse.GetType().GetProperty("rightButton")?.GetValue(mouse,null);
                        var isPressed = btn?.GetType().GetProperty("isPressed")?.GetValue(btn,null);
                        if (isPressed is bool b) return b;
                    }
                }
            } catch {}
            return Input.GetMouseButton(1);
        }
    }
}