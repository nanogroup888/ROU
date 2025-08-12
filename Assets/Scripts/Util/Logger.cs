using UnityEngine;

namespace ROLikeMMO.Util
{
    public static class Logger
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Log(string msg) => Debug.Log($"[ROLike] {msg}");
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Warn(string msg) => Debug.LogWarning($"[ROLike] {msg}");
        public static void Error(string msg) => Debug.LogError($"[ROLike] {msg}");
    }
}
