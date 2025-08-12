#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace ROLikeMMO.Tools
{
    public static class NetworkifySelectedMonsters
    {
        [MenuItem("ROLike Tools/Networkify Selected Monsters")]
        public static void Run()
        {
            var objs = Selection.objects;
            if (objs == null || objs.Length == 0)
            {
                EditorUtility.DisplayDialog("Select something", "Select one or more monster prefabs or scene objects, then run again.", "OK");
                return;
            }

            int ok = 0, fail = 0;
            foreach (var o in objs)
            {
                var go = o as GameObject;
                if (go == null)
                {
                    var path = AssetDatabase.GetAssetPath(o);
                    if (!string.IsNullOrEmpty(path))
                    {
                        var loaded = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (loaded != null) go = loaded;
                    }
                }
                if (go == null) { fail++; continue; }

                var pathAsset = AssetDatabase.GetAssetPath(go);
                if (!string.IsNullOrEmpty(pathAsset) && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
                {
                    var root = PrefabUtility.LoadPrefabContents(pathAsset);
                    try
                    {
                        Networkify(root);
                        PrefabUtility.SaveAsPrefabAsset(root, pathAsset);
                        ok++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[Networkify] Failed on {go.name}: {e.Message}");
                        fail++;
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
                else
                {
                    try
                    {
                        Networkify(go);
                        ok++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[Networkify] Failed on scene object {go.name}: {e.Message}");
                        fail++;
                    }
                }
            }

            EditorUtility.DisplayDialog("Networkify", $"Done.\nOK: {ok}\nFailed: {fail}\n\n• Ensure your Spawner uses these prefabs from the Project window.\n• Add them to NetworkManager → Registered Spawnable Prefabs.", "OK");
        }

        static void Networkify(GameObject root)
        {
            // Layer = Monster (recursive)
            int monsterLayer = LayerMask.NameToLayer("Monster");
            if (monsterLayer >= 0) SetLayerRecursive(root, monsterLayer);

            // Ensure NetworkIdentity on root only (via reflection to avoid hard dep if Mirror asm name differs)
            var niType = FindTypeByName("NetworkIdentity", "Mirror");
            if (niType == null) throw new Exception("Mirror.NetworkIdentity type not found. Is Mirror installed?");

            RemoveComponentsInChildrenButKeepOnRoot(root, niType);
            if (root.GetComponent(niType) == null) root.AddComponent(niType);

            // Ensure NetworkTransform on root using reflection (works across Mirror versions)
            var ntType = TryFindNetworkTransformType();
            if (ntType != null && root.GetComponent(ntType) == null) root.AddComponent(ntType);

            // Ensure Collider
            var col = root.GetComponent<Collider>();
            if (col == null)
            {
                var cc = root.AddComponent<CapsuleCollider>();
                cc.center = new Vector3(0, 1, 0);
                cc.height = 2f;
                cc.radius = 0.5f;
            }

            // Ensure Health + MonsterAI
            var healthType = FindTypeByFullName("ROLikeMMO.Gameplay.Health");
            if (healthType != null && root.GetComponent(healthType) == null) root.AddComponent(healthType);

            var aiType = FindTypeByFullName("ROLikeMMO.Gameplay.MonsterAI");
            if (aiType != null && root.GetComponent(aiType) == null) root.AddComponent(aiType);

            // HP bar anchor
            if (root.transform.Find("HealthBarAnchor") == null)
            {
                var anchor = new GameObject("HealthBarAnchor");
                anchor.transform.SetParent(root.transform, false);
                anchor.transform.localPosition = new Vector3(0, 1.8f, 0);
            }
        }

        static Type TryFindNetworkTransformType()
        {
            // Try common namespaces first
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
            // Fallback by name under any Mirror* namespace
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

        static void RemoveComponentsInChildrenButKeepOnRoot(GameObject root, Type type)
        {
            var comps = root.GetComponentsInChildren(type, true);
            foreach (var c in comps)
            {
                var comp = c as Component;
                if (comp != null && comp.gameObject != root)
                    UnityEngine.Object.DestroyImmediate(comp, true);
            }
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
                SetLayerRecursive(t.gameObject, layer);
        }
    }
}
#endif
