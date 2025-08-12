#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ROLike.Tools.Synty {
    public class CityQuickStartWizard : EditorWindow {
        [MenuItem("ROLike Tools/Synty/City Quick Start Wizard")]
        public static void ShowWindow(){
            GetWindow<CityQuickStartWizard>(false, "City Quick Start", true).minSize = new Vector2(420, 280);
        }

        public float gridSize = 2f;
        public int chunksX = 2;
        public int chunksZ = 2;
        public float chunkSizeMeters = 256f;
        public bool createRootHierarchy = true;
        public bool addGridGizmo = true;
        public float safeZoneRadius = 20f;

        public bool createDemoPath = true;
        public float demoRectHalf = 80f;
        public float demoCrossHalf = 60f;
        public int demoCornerSegments = 0;

        private void OnGUI(){
            EditorGUILayout.LabelField("City Base Setup", EditorStyles.boldLabel);
            gridSize = EditorGUILayout.FloatField("Grid Size (m)", gridSize);
            chunksX = EditorGUILayout.IntField("Chunks X", chunksX);
            chunksZ = EditorGUILayout.IntField("Chunks Z", chunksZ);
            chunkSizeMeters = EditorGUILayout.FloatField("Chunk Size (m)", chunkSizeMeters);
            createRootHierarchy = EditorGUILayout.Toggle("Create City Root Hierarchy", createRootHierarchy);
            addGridGizmo = EditorGUILayout.Toggle("Add GridGizmo to Root", addGridGizmo);
            safeZoneRadius = EditorGUILayout.FloatField("SafeZone Radius (m)", safeZoneRadius);

            if (GUILayout.Button("Create/Ensure Base Hierarchy")){
                CreateBase();
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Demo Path (for Road Helper)", EditorStyles.boldLabel);
            createDemoPath = EditorGUILayout.Toggle("Create Demo Path Objects", createDemoPath);
            demoRectHalf = EditorGUILayout.FloatField("Rect Half-Size (m)", demoRectHalf);
            demoCrossHalf = EditorGUILayout.FloatField("Cross Half-Size (m)", demoCrossHalf);
            demoCornerSegments = EditorGUILayout.IntSlider("Corner Split Segments", demoCornerSegments, 0, 4);

            if (GUILayout.Button("Generate Demo Path")){
                GenerateDemoPath();
            }
        }

        private void CreateBase(){
            var root = GameObject.Find("City_Root");
            if (root == null) {
                root = new GameObject("City_Root");
                Undo.RegisterCreatedObjectUndo(root, "Create City_Root");
            }
            string[] children = new string[]{ "Terrain_Ground", "Roads", "Buildings", "Props", "Interactables", "NPCs", "Triggers", "VFX_SFX", "Lighting", "UI_Worldspace" };
            foreach (var c in children){
                var child = GameObject.Find(c);
                if (child == null){
                    child = new GameObject(c);
                    Undo.RegisterCreatedObjectUndo(child, "Create "+c);
                }
                child.transform.SetParent(root.transform, true);
            }
            var safe = GameObject.Find("SafeZone");
            if (safe == null){
                safe = new GameObject("SafeZone");
                Undo.RegisterCreatedObjectUndo(safe, "Create SafeZone");
                safe.transform.position = Vector3.zero;
                var sc = safe.AddComponent<ROLike.Gameplay.SafeZone>();
                sc.radius = safeZoneRadius;
            }
            if (addGridGizmo){
                var giz = root.GetComponent<ROLike.Tools.GridGizmo>();
                if (giz == null) giz = root.AddComponent<ROLike.Tools.GridGizmo>();
                giz.cellSize = gridSize;
                giz.halfCells = Mathf.RoundToInt((chunksX*chunkSizeMeters*0.5f)/gridSize);
            }
            Selection.activeObject = root;
        }

        private void GenerateDemoPath(){
            var path = GameObject.Find("Path");
            if (path == null){
                path = new GameObject("Path");
                Undo.RegisterCreatedObjectUndo(path, "Create Path");
            }
            var toDel = new System.Collections.Generic.List<GameObject>();
            foreach (Transform t in path.transform) toDel.Add(t.gameObject);
            foreach (var go in toDel) Undo.DestroyObjectImmediate(go);

            CreatePoint(path.transform, "a", new Vector3(-demoRectHalf, 0f, 0f));
            CreatePoint(path.transform, "b", new Vector3(demoRectHalf, 0f, 0f));
            CreatePoint(path.transform, "c", new Vector3(demoRectHalf, 0f, demoRectHalf));
            CreatePoint(path.transform, "d", new Vector3(-demoRectHalf, 0f, demoRectHalf));
            CreatePoint(path.transform, "e", new Vector3(-demoRectHalf, 0f, 0f));

            CreatePoint(path.transform, "f", new Vector3(0f, 0f, 0f));
            CreatePoint(path.transform, "g", new Vector3(0f, 0f, demoCrossHalf));
            CreatePoint(path.transform, "h", new Vector3(0f, 0f, -demoCrossHalf));
            CreatePoint(path.transform, "i", new Vector3(-demoCrossHalf, 0f, 0f));
            CreatePoint(path.transform, "j", new Vector3(demoCrossHalf, 0f, 0f));

            foreach (Transform t in path.transform){
                var p = t.position;
                p.x = Mathf.Round(p.x / gridSize) * gridSize;
                p.z = Mathf.Round(p.z / gridSize) * gridSize;
                p.y = 0f;
                t.position = p;
            }
            Selection.activeObject = path;
        }

        private void CreatePoint(Transform parent, string name, Vector3 pos){
            var go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
        }
    }
}
#endif
