#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace ROLikeMMO.Tools
{
    public class FMegapackVariantMaker : EditorWindow
    {
        public enum ColliderMode { Auto, Force3D, Force2D }

        GameObject sourcePrefab;
        string saveFolder = "Assets/Prefabs/Monsters";
        ColliderMode colliderMode = ColliderMode.Force3D; // full 3D by default
        bool addHealthBarAnchor = true;

        [MenuItem("ROLike Tools/FantasyMonsters → Make Network Variant")]
        public static void Open() => GetWindow<FMegapackVariantMaker>("FMegapack Variant");

        void OnGUI()
        {
            sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Source Prefab", sourcePrefab, typeof(GameObject), false);
            saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
            colliderMode = (ColliderMode)EditorGUILayout.EnumPopup("Collider Mode", colliderMode);
            addHealthBarAnchor = EditorGUILayout.Toggle("Add HealthBar Anchor", addHealthBarAnchor);

            if (GUILayout.Button("Create Variant"))
            {
                try { CreateVariant(); }
                catch (Exception e)
                {
                    Debug.LogError("[FMegapackVariantMaker] ERROR: " + e.Message + "\n" + e.StackTrace);
                    EditorUtility.DisplayDialog("Error", e.Message, "OK");
                }
            }
        }

        void CreateVariant()
        {
            if (sourcePrefab == null) throw new Exception("Source Prefab is null.");
            string srcPath = AssetDatabase.GetAssetPath(sourcePrefab);
            if (string.IsNullOrEmpty(srcPath)) throw new Exception("Select a prefab from the Project window (not a scene object).");

            Log("Step 1: Ensure folder");
            EnsureFolder(saveFolder);

            string name = System.IO.Path.GetFileNameWithoutExtension(srcPath) + "_NET";
            string outPath = saveFolder.TrimEnd('/') + "/" + name + ".prefab";

            Log("Step 2: SaveAsPrefabAsset → " + outPath);
            var variant = PrefabUtility.SaveAsPrefabAsset(sourcePrefab, outPath);
            if (variant == null) throw new Exception("SaveAsPrefabAsset returned null.");

            Log("Step 3: LoadPrefabContents");
            var root = PrefabUtility.LoadPrefabContents(outPath);
            if (root == null) throw new Exception("LoadPrefabContents returned null.");

            try
            {
                Log("Step 4: Layer=Monster (recursive)");
                int monsterLayer = LayerMask.NameToLayer("Monster");
                if (monsterLayer >= 0) SetLayerRecursively(root, monsterLayer);
                else Debug.LogWarning("[FMV] Layer 'Monster' not found. Create it in Project Settings > Tags and Layers.");

                Log("Step 5: Ensure NetworkIdentity on ROOT");
                var niType = FindTypeByName("NetworkIdentity", "Mirror");
                if (niType == null) Debug.LogWarning("[FMV] Mirror.NetworkIdentity not found (is Mirror installed?).");
                else
                {
                    // remove NI on children (Mirror requires netId on root only)
                    foreach (var c in root.GetComponentsInChildren(niType, true))
                        if ((c as Component).gameObject != root)
                            UnityEngine.Object.DestroyImmediate(c as Component, true);
                    if (root.GetComponent(niType) == null) root.AddComponent(niType);
                }

                Log("Step 6: Ensure NetworkTransform (if available)");
                var ntType = TryFindNetworkTransformType();
                if (ntType != null)
                {
                    if (root.GetComponent(ntType) == null) root.AddComponent(ntType);
                }
                else Debug.LogWarning("[FMV] NetworkTransform type not found. Skipping.");

                Log("Step 7: Colliders — mode " + colliderMode);
                HandleColliders(root);

                Log("Step 8: Ensure Health + MonsterAI");
                var healthType = FindTypeByFullName("ROLikeMMO.Gameplay.Health");
                if (healthType != null && root.GetComponent(healthType) == null) root.AddComponent(healthType);
                var aiType = FindTypeByFullName("ROLikeMMO.Gameplay.MonsterAI");
                if (aiType != null && root.GetComponent(aiType) == null) root.AddComponent(aiType);

                Log("Step 9: HealthBarAnchor");
                if (addHealthBarAnchor && root.transform.Find("HealthBarAnchor") == null)
                {
                    var anchor = new GameObject("HealthBarAnchor");
                    anchor.transform.SetParent(root.transform, false);
                    anchor.transform.localPosition = new Vector3(0, 1.8f, 0);
                }

                Log("Step 10: SaveAsPrefabAsset");
                PrefabUtility.SaveAsPrefabAsset(root, outPath);
                Log("DONE: " + outPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
                AssetDatabase.Refresh();
            }
        }

        void HandleColliders(GameObject root)
        {
            bool has2D = root.GetComponentsInChildren<Collider2D>(true).Length > 0;
            bool has3D = root.GetComponentsInChildren<Collider>(true).Length > 0;

            switch (colliderMode)
            {
                case ColliderMode.Auto:
                    if (has2D && !has3D) { /* keep 2D */ }
                    else if (!has3D)
                    {
                        var col = root.AddComponent<CapsuleCollider>();
                        col.center = new Vector3(0, 1, 0);
                        col.height = 2f;
                        col.radius = 0.5f;
                    }
                    break;

                case ColliderMode.Force3D:
                    foreach (var c2d in root.GetComponentsInChildren<Collider2D>(true))
                        UnityEngine.Object.DestroyImmediate(c2d, true);
                    if (root.GetComponent<Collider>() == null)
                    {
                        var col = root.AddComponent<CapsuleCollider>();
                        col.center = new Vector3(0, 1, 0);
                        col.height = 2f;
                        col.radius = 0.5f;
                    }
                    break;

                case ColliderMode.Force2D:
                    foreach (var c3d in root.GetComponentsInChildren<Collider>(true))
                        UnityEngine.Object.DestroyImmediate(c3d, true);
                    if (root.GetComponent<Collider2D>() == null)
                    {
                        var col2d = root.AddComponent<CapsuleCollider2D>();
                        col2d.size = new Vector2(1f, 2f);
                        col2d.offset = new Vector2(0f, 1f);
                    }
                    break;
            }
        }

        static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            var parts = folderPath.Split('/');
            for (int i = 1; i < parts.Length; i++)
            {
                var parent = string.Join("/", parts, 0, i);
                var current = string.Join("/", parts, 0, i + 1);
                if (!AssetDatabase.IsValidFolder(current))
                    AssetDatabase.CreateFolder(parent, parts[i]);
            }
        }

        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
                SetLayerRecursively(t.gameObject, layer);
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

        static void Log(string msg) => Debug.Log("[FMegapackVariantMaker] " + msg);
    }
}
#endif
