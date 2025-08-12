// ------------------------------------------------
// File: Assets/Editor/ROLikeTools/SnapToGridWindow.cs
// ------------------------------------------------
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ROLike.Tools {
    public class SnapToGridWindow : EditorWindow {
        private const string kPrefKey = "ROLike_SnapToGrid";
        private float gridSize = 2f;
        private bool snapY = false;
        private float yValue = 0f;
        private bool autoSnapOnSelection = false;
        private float rotateSnap = 90f; // degrees
        private Vector3 offset = Vector3.zero;

        [MenuItem("ROLike Tools/Snap To Grid")]
        public static void ShowWindow(){
            var w = GetWindow<SnapToGridWindow>(false, "Snap To Grid", true);
            w.minSize = new Vector2(280, 160);
        }

        private void OnEnable(){
            gridSize = EditorPrefs.GetFloat(kPrefKey+"_grid", 2f);
            snapY = EditorPrefs.GetBool(kPrefKey+"_snapY", false);
            yValue = EditorPrefs.GetFloat(kPrefKey+"_y", 0f);
            autoSnapOnSelection = EditorPrefs.GetBool(kPrefKey+"_auto", false);
            rotateSnap = EditorPrefs.GetFloat(kPrefKey+"_rot", 90f);
            offset.x = EditorPrefs.GetFloat(kPrefKey+"_ox", 0f);
            offset.y = EditorPrefs.GetFloat(kPrefKey+"_oy", 0f);
            offset.z = EditorPrefs.GetFloat(kPrefKey+"_oz", 0f);
        }
        private void OnDisable(){
            EditorPrefs.SetFloat(kPrefKey+"_grid", gridSize);
            EditorPrefs.SetBool(kPrefKey+"_snapY", snapY);
            EditorPrefs.SetFloat(kPrefKey+"_y", yValue);
            EditorPrefs.SetBool(kPrefKey+"_auto", autoSnapOnSelection);
            EditorPrefs.SetFloat(kPrefKey+"_rot", rotateSnap);
            EditorPrefs.SetFloat(kPrefKey+"_ox", offset.x);
            EditorPrefs.SetFloat(kPrefKey+"_oy", offset.y);
            EditorPrefs.SetFloat(kPrefKey+"_oz", offset.z);
        }

        private void OnGUI(){
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
            gridSize = Mathf.Max(0.01f, EditorGUILayout.FloatField("Grid Size (m)", gridSize));
            offset = EditorGUILayout.Vector3Field("Offset", offset);
            snapY = EditorGUILayout.Toggle("Force Y", snapY);
            using (new EditorGUI.DisabledScope(!snapY)){
                yValue = EditorGUILayout.FloatField("Y Value", yValue);
            }
            rotateSnap = EditorGUILayout.FloatField("Rotation Snap (°)", rotateSnap);
            autoSnapOnSelection = EditorGUILayout.Toggle("Auto Snap On Selection", autoSnapOnSelection);

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(Selection.transforms.Length == 0)){
                if (GUILayout.Button($"Snap Selected ({Selection.transforms.Length})")){
                    SnapSelection();
                }
            }
        }

        private void OnSelectionChange(){
            if (!autoSnapOnSelection) return;
            SnapSelection();
        }

        private void SnapSelection(){
            Transform[] trs = Selection.transforms;
            if (trs == null || trs.Length == 0) return;
            Undo.RecordObjects(trs, "Snap To Grid");
            foreach (var t in trs){
                Vector3 p = t.position - offset;
                p.x = Mathf.Round(p.x / gridSize) * gridSize;
                p.z = Mathf.Round(p.z / gridSize) * gridSize;
                if (snapY) p.y = yValue; // fixed Y
                t.position = p + offset;

                Vector3 e = t.eulerAngles;
                e.y = Mathf.Round(e.y / rotateSnap) * rotateSnap;
                t.eulerAngles = e;
            }
        }
    }
}
#endif
