#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public static class RUW_MissingScriptsCleaner
{
    // ====== เมนูสำหรับ "ฉากที่เปิดอยู่" ======
    [MenuItem("ROLike Tools/Tools/Missing Scripts/Scan Active Scene")]
    public static void ScanActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        int missing = 0, objects = 0;
        foreach (var root in scene.GetRootGameObjects())
            missing += CountMissingOnHierarchy(root, ref objects);

        Debug.Log($"[Scan Active Scene] Found {missing} missing scripts on {objects} objects in scene '{scene.name}'.");
    }

    [MenuItem("ROLike Tools/Tools/Missing Scripts/Auto-Fix Active Scene (Remove)")]
    public static void FixActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        int removed = 0, objects = 0;

        Undo.IncrementCurrentGroup();
        foreach (var root in scene.GetRootGameObjects())
            removed += RemoveMissingOnHierarchy(root, ref objects);
        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Fix Active Scene] Removed {removed} missing scripts on {objects} objects in '{scene.name}'.");
    }

    // ====== เมนูสำหรับ "ทุก Prefab ในโปรเจกต์" ======
    [MenuItem("ROLike Tools/Tools/Missing Scripts/Scan All Prefabs")]
    public static void ScanAllPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int missing = 0, prefabs = 0, objs = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) continue;

            prefabs++;
            missing += CountMissingOnHierarchy(root, ref objs);
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log($"[Scan Prefabs] Found {missing} missing scripts across {objs} objects in {prefabs} prefabs.");
    }

    [MenuItem("ROLike Tools/Tools/Missing Scripts/Auto-Fix All Prefabs (Remove)")]
    public static void FixAllPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        int removed = 0, prefabs = 0, objs = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) continue;

            Undo.ClearAll(); // ป้องกัน Undo stack ทะลักตอนวนหลายไฟล์
            removed += RemoveMissingOnHierarchy(root, ref objs);

            if (removed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
            prefabs++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Fix Prefabs] Removed {removed} missing scripts across {objs} objects in {prefabs} prefabs.");
    }

    // ====== Helpers ======
    static int CountMissingOnHierarchy(GameObject root, ref int objectCount)
    {
        int missing = 0;
        foreach (var go in root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject))
        {
            var comps = go.GetComponents<Component>();
            int m = comps.Count(c => c == null);
            if (m > 0)
            {
                missing += m;
                objectCount++;
                Debug.LogWarning($"[Missing] {GetFullPath(go)}  (missing: {m})", go);
            }
        }
        return missing;
    }

    static int RemoveMissingOnHierarchy(GameObject root, ref int objectCount)
    {
        int removed = 0;
        foreach (var go in root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject))
        {
            int before = go.GetComponents<Component>().Count(c => c == null);
            if (before > 0)
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                int after = go.GetComponents<Component>().Count(c => c == null);
                int diff = before - after;
                if (diff > 0)
                {
                    removed += diff;
                    objectCount++;
                    Debug.Log($"[Removed] {diff} missing script(s) on {GetFullPath(go)}", go);
                }
            }
        }
        return removed;
    }

    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        var t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif
