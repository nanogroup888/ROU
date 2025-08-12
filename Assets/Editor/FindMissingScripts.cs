using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("ROLike Tools/Find Missing Scripts in Scene")]
    public static void FindMissing()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        foreach (var g in go)
        {
            var comps = g.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    Debug.LogWarning($"Missing script found in: {g.name}", g);
                    count++;
                }
            }
        }
        Debug.Log($"Total Missing Scripts: {count}");
    }
}
