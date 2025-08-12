using System;
using UnityEngine;

namespace ROLikeMMO.Util
{
    public static class Extensions
    {
        public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
        public static float Clamp01(this float v) => Mathf.Clamp01(v);
        public static int Clamp(this int v, int min, int max) => Mathf.Clamp(v, min, max);
        public static T Require<T>(this Component c) where T : Component
        {
            var r = c.GetComponent<T>();
            if (r == null) r = c.gameObject.AddComponent<T>();
            return r;
        }
    }
}
