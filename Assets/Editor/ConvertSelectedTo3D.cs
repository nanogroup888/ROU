#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace ROLikeMMO.Tools
{
    /// <summary>
    /// Convert selected prefabs/objects to 3D: remove 2D colliders, add CapsuleCollider (3D),
    /// ensure NetworkIdentity & NetworkTransform (reflection), set Layer=Monster (recursive).
    /// </summary>
    public static class ConvertSelectedTo3D
    {
        [MenuItem("ROLike Tools/Convert Selected To 3D (Collider + Network)")]
        public static void Run()
        {
            var objs = Selection.objects;
            if (objs == null || objs.Length == 0)
            {
                EditorUtility.DisplayDialog("Select something", "Select one or more prefabs or scene objects, then run again.", "OK");
                return;
            }

            int ok = 0, fail = 0;
            foreach (var o in objs)
            {
                var go = ToGameObject(o);
                if (go == null) { fail++; continue; }

                var path = AssetDatabase.GetAssetPath(go);
                if (!string.IsNullOrEmpty(path) && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
                {
                    var root = PrefabUtility.LoadPrefabContents(path);
                    try { Convert(root); PrefabUtility.SaveAsPrefabAsset(root, path); ok++; }
                    catch (System.Exception e) { Debug.LogError($"[ConvertTo3D] {go.name}: {e.Message}"); fail++; }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
                else
                {
                    try { Convert(go); ok++; }
                    catch (System.Exception e) { Debug.LogError($"[ConvertTo3D] scene obj {go.name}: {e.Message}"); fail++; }
                }
            }

            EditorUtility.DisplayDialog("Convert To 3D", $"OK: {ok}\nFailed: {fail}", "OK");
        }

        static GameObject ToGameObject(UnityEngine.Object o)
        {
            var go = o as GameObject;
            if (go) return go;
            var path = AssetDatabase.GetAssetPath(o);
            if (!string.IsNullOrEmpty(path))
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return null;
        }

        static void Convert(GameObject root)
        {
            // Remove 2D colliders
            foreach (var c2d in root.GetComponentsInChildren<Collider2D>(true))
                UnityEngine.Object.DestroyImmediate(c2d, true);

            // Ensure 3D collider on root
            if (root.GetComponent<Collider>() == null)
            {
                var col = root.AddComponent<CapsuleCollider>();
                col.center = new Vector3(0, 1, 0);
                col.height = 2f;
                col.radius = 0.5f;
            }

            // Layer = Monster
            int monsterLayer = LayerMask.NameToLayer("Monster");
            if (monsterLayer >= 0) SetLayerRecursive(root, monsterLayer);

            // NetworkIdentity on root, remove on children
            var niType = FindTypeByName("NetworkIdentity", "Mirror");
            if (niType == null) Debug.LogWarning("Mirror.NetworkIdentity not found.");
            else
            {
                foreach (var c in root.GetComponentsInChildren(niType, true))
                    if ((c as Component).gameObject != root)
                        UnityEngine.Object.DestroyImmediate(c as Component, true);
                if (root.GetComponent(niType) == null) root.AddComponent(niType);
            }

            // NetworkTransform (optional)
            var ntType = TryFindNetworkTransformType();
            if (ntType != null && root.GetComponent(ntType) == null) root.AddComponent(ntType);

            // Health + MonsterAI
            var healthT = FindTypeByFullName("ROLikeMMO.Gameplay.Health");
            if (healthT != null && root.GetComponent(healthT) == null) root.AddComponent(healthT);
            var aiT = FindTypeByFullName("ROLikeMMO.Gameplay.MonsterAI");
            if (aiT != null && root.GetComponent(aiT) == null) root.AddComponent(aiT);
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
                SetLayerRecursive(t.gameObject, layer);
        }

        static Type TryFindNetworkTransformType()
        {
            var candidates = new string[] {
                "Mirror.NetworkTransform",
                "Mirror.Components.NetworkTransform",
                "Mirror.Experimental.NetworkTransform"
            };
            foreach (var full in candidates)
            {
                var t = FindTypeByFullName(full);
                if (t != null) return t;
            }
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "NetworkTransform" && t.Namespace != null && t.Namespace.StartsWith("Mirror"));
        }

        static Type FindTypeByName(string typeName, string namespaceStartsWith)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == typeName && t.Namespace != null && t.Namespace.StartsWith(namespaceStartsWith));
        }

        static Type FindTypeByFullName(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.FullName == fullName);
        }
    }
}
#endif
