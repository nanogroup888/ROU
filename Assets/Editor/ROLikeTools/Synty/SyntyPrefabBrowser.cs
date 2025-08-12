#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ROLike.Tools.Synty {
    public class SyntyPrefabBrowser : EditorWindow {
        private string query = "fountain";
        private Vector2 scroll;
        private string[] searchFolders = new string[]{"Assets"};
        private List<string> results = new List<string>();

        [MenuItem("ROLike Tools/Synty/Prefab Browser")]
        public static void ShowWindow(){
            GetWindow<SyntyPrefabBrowser>(false, "Prefab Browser", true).minSize = new Vector2(520, 320);
        }

        private void OnGUI(){
            EditorGUILayout.LabelField("Search Prefabs", EditorStyles.boldLabel);
            query = EditorGUILayout.TextField("Query", query);
            EditorGUILayout.LabelField("Search Folders (one per line):");
            string joined = string.Join("\n", searchFolders);
            joined = EditorGUILayout.TextArea(joined, GUILayout.MinHeight(40));
            searchFolders = joined.Split(new[]{'\n','\r'}, System.StringSplitOptions.RemoveEmptyEntries);
            if (GUILayout.Button("Search")){
                Search();
            }
            EditorGUILayout.Space(6);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var path in results){
                using (new EditorGUILayout.HorizontalScope()){
                    EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<GameObject>(path), typeof(GameObject), false);
                    if (GUILayout.Button("Ping", GUILayout.Width(60))) EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
                    if (GUILayout.Button("Select", GUILayout.Width(60))) Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void Search(){
            results.Clear();
            var guids = AssetDatabase.FindAssets($"t:prefab {query}", searchFolders);
            foreach (var g in guids){
                results.Add(AssetDatabase.GUIDToAssetPath(g));
            }
        }
    }
}
#endif
