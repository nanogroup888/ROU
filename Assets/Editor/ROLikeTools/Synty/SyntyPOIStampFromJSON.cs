#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ROLike.Tools.Synty {

    [Serializable] public class SyntyRule { public string key; public string[] any; public string[] all; public string[] none; }
    [Serializable] public class SyntyRuleSet { public string[] rootSearchFolders; public List<SyntyRule> rules = new List<SyntyRule>(); }

    public class SyntyPOIStampFromJSON : EditorWindow {
        private string csvText = "name,x,z,notes\nGuildHall,120,60,กิลด์\nInn,160,-120,โรงเตี๊ยม\nMarketHall,-80,60,ตลาด\nFountain,6,-4,น้ำพุ\nSpawnStatue,0,0,เกิด\nTownHall,-20,-40,ทำเนียบ\nEastGate,240,-140,ประตู\nHarbor,180,-180,ท่า";
        private string jsonText = "";
        private float defaultY = 0f;
        private bool snapToGrid = true;
        private float gridSize = 2f;
        private Transform parent;
        private string[] searchFolders = new string[]{"Assets"};
        private Dictionary<string,UnityEngine.Object> resolved = new Dictionary<string,UnityEngine.Object>();

        [MenuItem("ROLike Tools/Synty/POI Stamper (JSON Auto Map)")]
        public static void ShowWindow(){
            GetWindow<SyntyPOIStampFromJSON>(false, "POI Stamper (JSON)", true).minSize = new Vector2(560, 520);
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
            csvText = EditorGUILayout.TextArea(csvText, GUILayout.MinHeight(100));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Mapping JSON (rules)", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope()){
                if (GUILayout.Button("Load JSON…", GUILayout.Width(110))){
                    var p = EditorUtility.OpenFilePanel("Load JSON", Application.dataPath, "json");
                    if (!string.IsNullOrEmpty(p)) jsonText = File.ReadAllText(p);
                }
                if (GUILayout.Button("Fill with Default (Fantasy Kingdom)", GUILayout.Width(280))){
                    jsonText = GetDefaultRuleJSON();
                }
            }
            jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.MinHeight(120));

            defaultY = EditorGUILayout.FloatField("Default Y", defaultY);
            snapToGrid = EditorGUILayout.Toggle("Snap To Grid", snapToGrid);
            if (snapToGrid) gridSize = EditorGUILayout.FloatField("Grid Size (m)", gridSize);
            parent = (Transform)EditorGUILayout.ObjectField("Parent (optional)", parent, typeof(Transform), true);

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Auto-Find Prefabs (Preview)")){
                AutoFindPrefabs();
            }

            if (resolved.Count > 0){
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Resolved Mappings", EditorStyles.boldLabel);
                var keys = resolved.Keys.ToList();
                keys.Sort();
                foreach (var k in keys){
                    UnityEngine.Object obj = resolved[k];
                    using (new EditorGUILayout.HorizontalScope()){
                        EditorGUILayout.LabelField(k, GUILayout.Width(150));
                        obj = EditorGUILayout.ObjectField(obj, typeof(GameObject), false);
                        resolved[k] = obj;
                    }
                }
            }

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Stamp POIs")){
                Stamp();
            }
        }

        private void AutoFindPrefabs(){
            resolved.Clear();
            SyntyRuleSet set = null;
            try { set = JsonUtility.FromJson<SyntyRuleSet>(jsonText); } catch { set = null; }
            if (set == null || set.rules == null || set.rules.Count == 0){
                EditorUtility.DisplayDialog("JSON", "JSON ไม่ถูกต้องหรือไม่มีกฎ", "OK");
                return;
            }
            if (set.rootSearchFolders != null && set.rootSearchFolders.Length > 0) searchFolders = set.rootSearchFolders;

            foreach (var rule in set.rules){
                string chosenGuid = null;
                float bestScore = -1f;

                HashSet<string> candGuids = new HashSet<string>();
                if (rule.any != null && rule.any.Length > 0){
                    foreach (var token in rule.any){
                        var guids = AssetDatabase.FindAssets($"t:prefab {token}", searchFolders);
                        foreach (var g in guids) candGuids.Add(g);
                    }
                } else {
                    foreach (var g in AssetDatabase.FindAssets("t:prefab", searchFolders)) candGuids.Add(g);
                }

                foreach (var guid in candGuids){
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();

                    bool allOk = true;
                    if (rule.all != null){
                        foreach (var t in rule.all){
                            if (!name.Contains(t.ToLowerInvariant())) { allOk = false; break; }
                        }
                    }
                    if (!allOk) continue;

                    bool noneOk = true;
                    if (rule.none != null){
                        foreach (var t in rule.none){
                            if (name.Contains(t.ToLowerInvariant())) { noneOk = false; break; }
                        }
                    }
                    if (!noneOk) continue;

                    float score = 0f;
                    if (rule.any != null){
                        foreach (var t in rule.any){
                            if (name.Contains(t.ToLowerInvariant())) score += 1f;
                        }
                    }
                    var pathLower = path.ToLowerInvariant();
                    if (pathLower.Contains("polygon") || pathLower.Contains("fantasy") || pathLower.Contains("kingdom")) score += 0.5f;

                    if (score > bestScore){
                        bestScore = score;
                        chosenGuid = guid;
                    }
                }

                if (!string.IsNullOrEmpty(chosenGuid)){
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(chosenGuid));
                    resolved[rule.key] = obj;
                } else {
                    resolved[rule.key] = null;
                }
            }
        }

        private void Stamp(){
            if (string.IsNullOrWhiteSpace(csvText)) return;
            var lines = csvText.Split(new[]{'\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
            var created = new List<GameObject>();

            foreach (var raw in lines){
                var line = raw.Trim();
                if (line.StartsWith("#") || line.StartsWith("//")) continue;
                if (line.ToLowerInvariant().StartsWith("name,")) continue;
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

                GameObject prefab = resolved.ContainsKey(name) ? (GameObject)resolved[name] : null;
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

        private string GetDefaultRuleJSON(){
            var rs = new SyntyRuleSet{
                rootSearchFolders = new string[]{ "Assets" },
                rules = new List<SyntyRule>{
                    new SyntyRule{ key="GuildHall", any=new[]{"guild","hall"}, none=new[]{"stall","shop"} },
                    new SyntyRule{ key="MarketHall", any=new[]{"market","shop","stall"} },
                    new SyntyRule{ key="Inn", any=new[]{"inn","tavern"} },
                    new SyntyRule{ key="TownHall", any=new[]{"town","hall","civic"} },
                    new SyntyRule{ key="Fountain", any=new[]{"fountain"} },
                    new SyntyRule{ key="SpawnStatue", any=new[]{"statue"} },
                    new SyntyRule{ key="EastGate", any=new[]{"gate","gatehouse"} },
                    new SyntyRule{ key="Harbor", any=new[]{"dock","harbor","pier","port"} }
                }
            };
            return JsonUtility.ToJson(rs, true);
        }
    }
}
#endif
