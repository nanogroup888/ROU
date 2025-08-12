// ------------------------------------------------
// File: Assets/Editor/ROLikeTools/RoadHelperWindow.cs
// ------------------------------------------------
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace ROLike.Tools {
    public class RoadHelperWindow : EditorWindow {
        private Transform path;
        private GameObject straightSegmentPrefab; // length along Z, width along X
        private float segmentLength = 2f;
        private string outputParentName = "Road_Generated";
        private bool yawOnly = true;
        private bool clearOldChildren = true;

        [MenuItem("ROLike Tools/Road Helper (From Path)")]
        public static void ShowWindow(){
            GetWindow<RoadHelperWindow>(false, "Road Helper", true).minSize = new Vector2(380, 220);
        }

        private void OnGUI(){
            EditorGUILayout.LabelField("Path → Segments", EditorStyles.boldLabel);
            path = (Transform)EditorGUILayout.ObjectField("Path (Parent)", path, typeof(Transform), true);
            straightSegmentPrefab = (GameObject)EditorGUILayout.ObjectField("Straight Prefab", straightSegmentPrefab, typeof(GameObject), false);
            segmentLength = EditorGUILayout.FloatField("Segment Length (m)", segmentLength);
            yawOnly = EditorGUILayout.Toggle("Yaw Only Rotation", yawOnly);
            clearOldChildren = EditorGUILayout.Toggle("Clear Old Output", clearOldChildren);
            outputParentName = EditorGUILayout.TextField("Output Parent", outputParentName);

            using (new EditorGUI.DisabledScope(path == null || straightSegmentPrefab == null)){
                if (GUILayout.Button("Generate Road")) Generate();
            }

            EditorGUILayout.HelpBox("วิธีใช้: สร้าง GameObject ชื่อ Path แล้ววางลูก (A,B,C,...) เป็นจุดตามเส้นทางบนพื้น จากนั้นเลือก Path และ Prefab แล้วกด Generate.", MessageType.Info);
        }

        private void Generate(){
            if (path == null || straightSegmentPrefab == null) return;
            if (path.childCount < 2){
                EditorUtility.DisplayDialog("Road Helper", "ต้องมีจุดอย่างน้อย 2 จุดใน Path", "OK");
                return;
            }
            var container = GameObject.Find(outputParentName) ?? new GameObject(outputParentName);
            Undo.RegisterCreatedObjectUndo(container, "Create Road Container");

            if (clearOldChildren){
                var olds = new List<GameObject>();
                foreach (Transform c in container.transform) olds.Add(c.gameObject);
                foreach (var go in olds) Undo.DestroyObjectImmediate(go);
            }

            var nodes = new List<Transform>();
            foreach (Transform t in path) nodes.Add(t);
            nodes.Sort((a,b)=> a.GetSiblingIndex().CompareTo(b.GetSiblingIndex()));

            int total = 0;
            for (int i=0;i<nodes.Count-1;i++){
                Vector3 a = nodes[i].position;
                Vector3 b = nodes[i+1].position;
                Vector3 dir = (b - a);
                float dist = dir.magnitude;
                if (dist < 0.001f) continue;
                dir.Normalize();
                int steps = Mathf.Max(1, Mathf.RoundToInt(dist / segmentLength));
                float step = dist / steps;

                for (int s=0; s<steps; s++){
                    Vector3 p = a + dir * (step * (s + 0.5f)); // center of segment
                    Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                    if (yawOnly) rot = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(straightSegmentPrefab);
                    Undo.RegisterCreatedObjectUndo(inst, "Create Road Segment");
                    inst.transform.SetPositionAndRotation(p, rot);
                    inst.transform.SetParent(container.transform, true);
                    inst.name = $"RoadSeg_{i}_{s}";
                    total++;
                }
            }
            EditorUtility.DisplayDialog("Road Helper", $"สร้างถนนสำเร็จ: {total} ชิ้น", "OK");
            Selection.activeObject = container;
        }
    }
}
#endif
