// ------------------------------------------------
// File: Assets/Editor/ROLikeTools/POIStamperWindow.cs
// ------------------------------------------------
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace ROLike.Tools {
    [Serializable]
    public class PrefabMappingEntry {
        public string key; // e.g. name or logical key
        public GameObject prefab;
    }

    public class POIStamperWindow : EditorWindow {
        private const string kTitle = "POI Stamper";
        private string csvText = "name,x,z,notes\nSpawnStatue,0,0,จุดเกิด\nFountain,6,-4,น้ำพุ\nGuildHall,120,60,ฮอลล์กิลด์\nInn,160,-120,โรงเตี๊ยม";
        private float defaultY = 0f;
        private bool snapToGrid = true;
        private float gridSize = 2f;
        private Transform parent;
        private List<PrefabMappingEntry> mappings = new List<PrefabMappingEntry>();

        [MenuItem("ROLike Tools/POI Stamper")]
        public static void ShowWindow(){
            GetWindow<POIStamperWindow>(false, kTitle, true).minSize = new Vector2(460, 380);
        }

        private void OnGUI(){
            EditorGUILayout.LabelField("CSV (name,x,z,notes[,y])", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope()){
                if (GUILayout.Button("Load CSV…", GUILayout.Width(110))){
                    var p = EditorUtility.OpenFilePanel("Load CSV", Application.dataPath, "csv");
                    if (!string.IsNullOrEmpty(p)) csvText = File.ReadAllText(p);
                }
                if (GUILayout.Button("Clear", GUILayout.Width(80))) csvText = string.Empty;
            }
            csvText = EditorGUILayout.TextArea(csvText, GUILayout.MinHeight(140));

            EditorGUILayout.Space();
            defaultY = EditorGUILayout.FloatField("Default Y", defaultY);
            snapToGrid = EditorGUILayout.Toggle("Snap To Grid", snapToGrid);
            if (snapToGrid) gridSize = EditorGUILayout.FloatField("Grid Size (m)", gridSize);
            parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Prefab Mappings (optional)", EditorStyles.boldLabel);
            int remove = -1;
            for (int i=0; i<mappings.Count; i++){
                using (new EditorGUILayout.HorizontalScope()){
                    mappings[i].key = EditorGUILayout.TextField(mappings[i].key);
                    mappings[i].prefab = (GameObject)EditorGUILayout.ObjectField(mappings[i].prefab, typeof(GameObject), false);
                    if (GUILayout.Button("-", GUILayout.Width(24))) remove = i;
                }
            }
            if (remove >= 0) mappings.RemoveAt(remove);
            if (GUILayout.Button("+ Add Mapping")) mappings.Add(new PrefabMappingEntry());

            EditorGUILayout.Space();
            if (GUILayout.Button("Stamp POIs")) Stamp();
        }

        private void Stamp(){
            if (string.IsNullOrWhiteSpace(csvText)) return;
            var lines = csvText.Split(new[]{'\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
            var created = new List<GameObject>();

            foreach (var raw in lines){
                var line = raw.Trim();
                if (line.StartsWith("#") || line.StartsWith("//")) continue;
                var lower = line.ToLowerInvariant();
                if (lower.StartsWith("name,")) continue; // header
                var cols = line.Split(',');
                if (cols.Length < 3) continue;

                string name = cols[0].Trim();
                float x = ParseFloat(cols[1]);
                float z = ParseFloat(cols[2]);
                float y = defaultY;
                if (cols.Length >= 5) y = ParseFloat(cols[4]);

                if (snapToGrid){
                    x = Mathf.Round(x / gridSize) * gridSize;
                    z = Mathf.Round(z / gridSize) * gridSize;
                }

                GameObject prefab = ResolvePrefab(name);
                GameObject go;
                if (prefab != null){
                    go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                } else {
                    go = new GameObject(name);
                }
                Undo.RegisterCreatedObjectUndo(go, "Create POI");
                go.name = name;
                go.transform.position = new Vector3(x, y, z);
                if (parent != null) go.transform.SetParent(parent, true);
                created.Add(go);
            }
            Selection.objects = created.ToArray();
        }

        private float ParseFloat(string s){
            if (float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v;
            if (float.TryParse(s, out v)) return v;
            return 0f;
        }

        private GameObject ResolvePrefab(string key){
            foreach (var m in mappings){
                if (!string.IsNullOrEmpty(m.key) && string.Equals(m.key, key, StringComparison.OrdinalIgnoreCase))
                    return m.prefab;
            }
            return null;
        }
    }
}
#endif
