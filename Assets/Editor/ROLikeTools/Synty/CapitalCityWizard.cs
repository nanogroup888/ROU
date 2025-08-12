// ------------------------------------------------
// File: Assets/Editor/ROLikeTools/Synty/CapitalCityWizard.cs
// Purpose: Turn the current Synty demo scene into your game's Capital City
// Menu: ROLike Tools → Synty → Capital City Wizard
// ------------------------------------------------
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ROLike.Tools.Synty
{
    public class CapitalCityWizard : EditorWindow
    {
        [MenuItem("ROLike Tools/Synty/Capital City Wizard")]
        public static void ShowWindow(){
            GetWindow<CapitalCityWizard>(false, "Capital City", true).minSize = new Vector2(520, 520);
        }

        // Scene Save
        private string sceneSaveFolder = "Assets/Scenes";
        private string sceneFileName = "Capital_City.unity";

        // Center & Safe Zone
        private Transform centerObject;
        private float safeZoneRadius = 20f;

        // Spawn ring
        private int spawnPoints = 6;
        private float spawnRingRadius = 8f;
        private bool addMirrorStartPositions = true;

        // Grid
        private float gridSize = 2f;

        // POIs
        private Transform poisParent;
        private List<string> poiKeys = new List<string>{ "SpawnStatue", "Fountain", "GuildHall", "MarketHall", "TownHall", "Inn", "EastGate", "Harbor" };

        private Vector2 scroll;

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("1) Save current scene as 'Capital City'", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                sceneSaveFolder = EditorGUILayout.TextField("Save Folder", sceneSaveFolder);
                if (GUILayout.Button("Pick", GUILayout.Width(60)))
                {
                    var p = EditorUtility.OpenFolderPanel("Choose Scene Folder", "Assets", "");
                    if (!string.IsNullOrEmpty(p) && p.StartsWith(Application.dataPath))
                    {
                        sceneSaveFolder = "Assets" + p.Substring(Application.dataPath.Length);
                    }
                }
            }
            sceneFileName = EditorGUILayout.TextField("File Name", sceneFileName);
            if (GUILayout.Button("Save As Capital Scene"))
            {
                SaveAsCapitalScene();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("2) Choose Capital Center", EditorStyles.boldLabel);
            centerObject = (Transform)EditorGUILayout.ObjectField("Center Object", centerObject, typeof(Transform), true);
            if (centerObject == null)
            {
                if (GUILayout.Button("Find Candidates (fountain/statue/plaza/square)"))
                {
                    var candidates = FindCenterCandidates();
                    if (candidates.Count > 0)
                    {
                        Selection.objects = candidates.ToArray();
                        EditorUtility.DisplayDialog("Candidates", $"เลือกหนึ่งชิ้นใน Hierarchy แล้วกลับมาวางในช่อง Center Object", "OK");
                    }
                    else EditorUtility.DisplayDialog("Candidates", "ไม่พบวัตถุที่น่าจะเป็นจุดศูนย์กลาง ลองเลือกด้วยตนเอง", "OK");
                }
            }
            safeZoneRadius = EditorGUILayout.FloatField("Safe Zone Radius (m)", safeZoneRadius);
            if (GUILayout.Button("Place/Update SafeZone at Center"))
            {
                PlaceSafeZone();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("3) Spawn Points", EditorStyles.boldLabel);
            spawnPoints = EditorGUILayout.IntSlider("Count", spawnPoints, 1, 24);
            spawnRingRadius = EditorGUILayout.FloatField("Ring Radius (m)", spawnRingRadius);
            addMirrorStartPositions = EditorGUILayout.Toggle("Add Mirror NetworkStartPosition (if available)", addMirrorStartPositions);
            if (GUILayout.Button("Create/Update Spawn Ring"))
            {
                CreateSpawnRing();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("4) Scene Hierarchy & Grid", EditorStyles.boldLabel);
            gridSize = EditorGUILayout.FloatField("Grid Size (m)", gridSize);
            if (GUILayout.Button("Ensure City_Root Hierarchy"))
            {
                EnsureCityRoot();
            }
            if (GUILayout.Button("Snap major groups to grid"))
            {
                SnapMajorGroupsToGrid();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("5) POIs", EditorStyles.boldLabel);
            poisParent = (Transform)EditorGUILayout.ObjectField("POIs Parent", poisParent, typeof(Transform), true);
            if (poisParent == null && GUILayout.Button("Create 'POIs' Root under City_Root"))
            {
                var root = GameObject.Find("City_Root");
                if (root == null) root = new GameObject("City_Root");
                var go = new GameObject("POIs");
                go.transform.SetParent(root.transform, true);
                poisParent = go.transform;
                Selection.activeObject = go;
            }
            EditorGUILayout.LabelField("POI Keys:");
            for (int i=0;i<poiKeys.Count;i++)
                poiKeys[i] = EditorGUILayout.TextField($"[{i+1}]", poiKeys[i]);
            if (GUILayout.Button("Add POI placeholder at Center for each key"))
            {
                AddPOIPlaceholders();
            }
            if (GUILayout.Button("Export POIs CSV to Assets/Configs/POIs_Capital.csv"))
            {
                ExportPOIsCSV();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("6) Tags/Layers", EditorStyles.boldLabel);
            if (GUILayout.Button("Ensure Tags/Layers (Vendor/QuestGiver/Inn/Guild/Auction, Layers: Ground/Building/Prop/Interactable/NPC/NoNav/Water)"))
            {
                TagLayerUtility.EnsureDefaults();
            }

            EditorGUILayout.HelpBox(
                "ขั้นตอน: เซฟซีน → เลือก Center → วาง SafeZone → วาง Spawn Ring → จัดโครง City_Root → เพิ่ม POIs → Export CSV → ใช้ POI Stamper/JSON map กับพรีแฟบ Synty.\n" +
                "หมายเหตุ: ถ้ามี Mirror จะใส่ NetworkStartPosition ให้อัตโนมัติ (ถ้าปิดติ๊ก)",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void SaveAsCapitalScene()
        {
            if (!Directory.Exists(sceneSaveFolder)) Directory.CreateDirectory(sceneSaveFolder);
            string path = Path.Combine(sceneSaveFolder, sceneFileName).Replace("\\","/");
            if (EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path))
            {
                EditorUtility.DisplayDialog("Capital Scene", $"บันทึกซีนที่: {path}", "OK");
            }
            else EditorUtility.DisplayDialog("Capital Scene", "บันทึกไม่สำเร็จ", "OK");
        }

        private List<GameObject> FindCenterCandidates()
        {
            string[] tokens = new[]{"fountain","statue","plaza","square"};
            var all = GameObject.FindObjectsOfType<Transform>(true);
            var list = new List<GameObject>();
            foreach (var t in all)
            {
                string n = t.name.ToLowerInvariant();
                if (tokens.Any(tok => n.Contains(tok)))
                    list.Add(t.gameObject);
            }
            return list;
        }

        private Vector3 GetCenterPosition()
        {
            if (centerObject != null) return centerObject.position;
            return Vector3.zero;
        }

        private void PlaceSafeZone()
        {
            var center = GetCenterPosition();
            var safe = GameObject.Find("SafeZone");
            if (safe == null) safe = new GameObject("SafeZone");
            var sc = safe.GetComponent<ROLike.Gameplay.SafeZone>();
            if (sc == null) sc = safe.AddComponent<ROLike.Gameplay.SafeZone>();
            sc.radius = safeZoneRadius;
            safe.transform.position = center;
            var root = GameObject.Find("City_Root") ?? new GameObject("City_Root");
            safe.transform.SetParent(root.transform, true);
            Selection.activeObject = safe;
        }

        private void CreateSpawnRing()
        {
            var center = GetCenterPosition();
            var parent = GameObject.Find("Spawn_Ring") ?? new GameObject("Spawn_Ring");
            parent.transform.position = center;
            var root = GameObject.Find("City_Root") ?? new GameObject("City_Root");
            parent.transform.SetParent(root.transform, true);

            // Clear old
            var toDel = new List<GameObject>();
            foreach (Transform t in parent.transform) toDel.Add(t.gameObject);
            foreach (var go in toDel) Undo.DestroyObjectImmediate(go);

            // Mirror detection
            System.Type mirrorStartType = null;
            if (addMirrorStartPositions)
            {
                mirrorStartType = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a=>{ try { return a.GetTypes(); } catch { return new System.Type[0]; } })
                    .FirstOrDefault(t => t.FullName == "Mirror.NetworkStartPosition");
            }

            for (int i=0;i<spawnPoints;i++)
            {
                float ang = (Mathf.PI*2f) * (i/(float)spawnPoints);
                var p = center + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * spawnRingRadius;
                var go = new GameObject($"Spawn_{i+1:00}");
                go.transform.SetParent(parent.transform, true);
                go.transform.position = p;
                go.tag = "SpawnPoint";

                if (mirrorStartType != null)
                {
                    go.AddComponent(mirrorStartType);
                }
            }
            Selection.activeObject = parent;
        }

        private void EnsureCityRoot()
        {
            var root = GameObject.Find("City_Root");
            if (root == null) root = new GameObject("City_Root");
            string[] children = new string[]{ "Terrain_Ground", "Roads", "Buildings", "Props", "Interactables", "NPCs", "Triggers", "VFX_SFX", "Lighting", "UI_Worldspace" };
            foreach (var c in children){
                var child = GameObject.Find(c);
                if (child == null){
                    child = new GameObject(c);
                }
                child.transform.SetParent(root.transform, true);
            }
            Selection.activeObject = root;
        }

        private void SnapMajorGroupsToGrid()
        {
            float s = Mathf.Max(0.01f, gridSize);
            var names = new[]{ "Roads", "Buildings", "Props" };
            foreach (var n in names){
                var go = GameObject.Find(n);
                if (go == null) continue;
                foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                {
                    if (t == go.transform) continue;
                    var p = t.position;
                    p.x = Mathf.Round(p.x / s) * s;
                    p.z = Mathf.Round(p.z / s) * s;
                    t.position = p;
                }
            }
        }

        private void AddPOIPlaceholders()
        {
            if (poisParent == null)
            {
                var root = GameObject.Find("City_Root") ?? new GameObject("City_Root");
                var go = new GameObject("POIs");
                go.transform.SetParent(root.transform, true);
                poisParent = go.transform;
            }
            var center = GetCenterPosition();
            float step = 4f;
            for (int i=0;i<poiKeys.Count;i++)
            {
                var name = poiKeys[i].Trim();
                if (string.IsNullOrEmpty(name)) continue;
                var go = new GameObject(name);
                go.transform.position = center + new Vector3((i%4)*step, 0f, (i/4)*step);
                go.transform.SetParent(poisParent, true);
            }
            Selection.activeObject = poisParent.gameObject;
        }

        private void ExportPOIsCSV()
        {
            if (poisParent == null)
            {
                EditorUtility.DisplayDialog("POIs", "ยังไม่ได้สร้าง POIs Parent", "OK");
                return;
            }
            string folder = "Assets/Configs";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "POIs_Capital.csv").Replace("\\","/");
            using (var w = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                w.WriteLine("name,x,z,notes");
                foreach (Transform t in poisParent)
                {
                    var p = t.position;
                    w.WriteLine($"{t.name},{p.x:F3},{p.z:F3},");
                }
            }
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("POIs", $"บันทึก CSV: {path}", "OK");
        }
    }

    // ----------------------- Tag/Layer helper -----------------------
    public static class TagLayerUtility
    {
        public static void EnsureDefaults()
        {
            EnsureTags(new[]{"SpawnPoint","Vendor","QuestGiver","Inn","Guild","Auction","Gate"});
            EnsureLayers(new[]{"Ground","Building","Prop","Interactable","NPC","NoNav","Water"});
            EditorUtility.DisplayDialog("Tags/Layers", "Ensured default tags & layers", "OK");
        }

        public static void EnsureTags(IEnumerable<string> tags)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");
            foreach (var t in tags)
            {
                if (!HasStringInArray(tagsProp, t))
                {
                    AddStringToArray(tagsProp, t);
                }
            }
            tagManager.ApplyModifiedProperties();
        }

        public static void EnsureLayers(IEnumerable<string> layers)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");
            // user layers start at 8
            int slot = 8;
            foreach (var l in layers)
            {
                if (!HasStringInArray(layersProp, l))
                {
                    // find empty slot
                    while (slot < layersProp.arraySize && !string.IsNullOrEmpty(layersProp.GetArrayElementAtIndex(slot).stringValue)) slot++;
                    if (slot >= layersProp.arraySize) break;
                    layersProp.GetArrayElementAtIndex(slot).stringValue = l;
                    slot++;
                }
            }
            tagManager.ApplyModifiedProperties();
        }

        private static bool HasStringInArray(SerializedProperty arrayProp, string s)
        {
            for (int i=0;i<arrayProp.arraySize;i++)
            {
                var p = arrayProp.GetArrayElementAtIndex(i);
                if (p != null && p.stringValue == s) return true;
            }
            return false;
        }
        private static void AddStringToArray(SerializedProperty arrayProp, string s)
        {
            arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
            arrayProp.GetArrayElementAtIndex(arrayProp.arraySize-1).stringValue = s;
        }
    }
}
#endif
