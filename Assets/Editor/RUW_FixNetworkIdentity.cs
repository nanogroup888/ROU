#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public static class RUW_FixNetworkIdentity
{
    // สแกน Prefab ใน Assets (ที่คุณรันไปแล้ว)
    [MenuItem("ROLike Tools/Networking/Scan Prefabs for Child NetworkIdentity")]
    public static void ScanPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int problems = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab) continue;
            var nis = prefab.GetComponentsInChildren<Mirror.NetworkIdentity>(true);
            if (nis.Length > 1 || (nis.Length == 1 && nis[0].gameObject != prefab))
            {
                problems++;
                Debug.LogWarning($"[Scan Prefab] {path} has invalid NetworkIdentity placement.", prefab);
            }
        }
        Debug.Log($"Scan complete. Prefabs with issues: {problems}");
    }

    // ✅ สแกนวัตถุใน "ฉากที่เปิดอยู่ตอนนี้"
    [MenuItem("ROLike Tools/Networking/Scan Active Scene for Child NetworkIdentity")]
    public static void ScanActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        int problems = 0;
        foreach (var root in roots)
        {
            var nis = root.GetComponentsInChildren<Mirror.NetworkIdentity>(true);
            // ต้องมี NI แค่ตัวเดียวบน root เท่านั้น
            if (nis.Length > 1)
            {
                problems++;
                Debug.LogError($"[Scene] '{root.name}' has {nis.Length} NetworkIdentity components (must be 1 on root).", root);
            }
            else if (nis.Length == 1 && nis[0].gameObject != root)
            {
                problems++;
                Debug.LogError($"[Scene] '{root.name}' has NetworkIdentity on a child ('{nis[0].gameObject.name}'). Move it to root.", root);
            }
        }
        Debug.Log($"Scan active scene complete. Objects with issues: {problems}");
    }

    // ⚠️ ตัวช่วยแก้อัตโนมัติในฉาก (จะลบ NI บนลูกและเพิ่มให้ root ถ้ายังไม่มี)
    [MenuItem("ROLike Tools/Networking/Auto-fix Active Scene NetworkIdentity")]
    public static void AutoFixActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        int fixedCount = 0;

        foreach (var root in roots)
        {
            var nis = root.GetComponentsInChildren<Mirror.NetworkIdentity>(true);
            if (nis.Length == 0)
                continue;

            // ให้แน่ใจว่า root มี NI
            var rootNI = root.GetComponent<Mirror.NetworkIdentity>();
            if (!rootNI)
            {
                Undo.AddComponent<Mirror.NetworkIdentity>(root);
                fixedCount++;
                Debug.Log($"[Fix Scene] Added NetworkIdentity to root: {root.name}", root);
            }

            // ลบ NI บนลูกทั้งหมด
            foreach (var ni in nis)
            {
                if (ni.gameObject != root)
                {
                    Undo.RecordObject(ni.gameObject, "Remove child NetworkIdentity");
                    Object.DestroyImmediate(ni, true);
                    fixedCount++;
                    Debug.Log($"[Fix Scene] Removed child NetworkIdentity on '{ni.gameObject.name}' under '{root.name}'", root);
                }
            }
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"Auto-fix scene complete. Changes: {fixedCount}");
    }
}
#endif
