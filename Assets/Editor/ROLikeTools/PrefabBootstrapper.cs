// ------------------------------------------------
// File: Assets/Editor/ROLikeTools/PrefabBootstrapper.cs
// ------------------------------------------------
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ROLike.Tools {
    public class PrefabBootstrapper : EditorWindow {
        private string baseFolder = "Assets/Prefabs";
        private float roadThickness = 0.1f;
        private Material roadMat;
        private Material buildingMat;
        private Material landmarkMat;

        [MenuItem("ROLike Tools/Create Starter Prefabs")]
        public static void ShowWindow(){
            GetWindow<PrefabBootstrapper>(false, "Starter Prefabs", true).minSize = new Vector2(380, 240);
        }

        private void OnGUI(){
            baseFolder = EditorGUILayout.TextField("Save Folder", baseFolder);
            roadThickness = EditorGUILayout.FloatField("Road Thickness", roadThickness);
            roadMat = (Material)EditorGUILayout.ObjectField("Road Material", roadMat, typeof(Material), false);
            buildingMat = (Material)EditorGUILayout.ObjectField("Building Material", buildingMat, typeof(Material), false);
            landmarkMat = (Material)EditorGUILayout.ObjectField("Landmark Material", landmarkMat, typeof(Material), false);

            if (GUILayout.Button("Generate Minimal Set")){
                GenerateAll();
            }

            EditorGUILayout.HelpBox("จะสร้าง Prefab พื้นฐาน: ถนนตรง 2m (กว้าง 4m/8m), อาคารเปลือก Guild/Market/Inn, รูปปั้น/น้ำพุ (Placeholder)", MessageType.Info);
        }

        private void GenerateAll(){
            EnsureFolderRecursive(baseFolder);
            EnsureFolderRecursive("Assets/Art/Materials");

            var matRoad = roadMat ?? CreateFlatMaterial("M_Road_Dark", new Color(0.18f,0.18f,0.18f));
            var matBld = buildingMat ?? CreateFlatMaterial("M_Building_Stone", new Color(0.75f,0.75f,0.78f));
            var matLM = landmarkMat ?? CreateFlatMaterial("M_Landmark", new Color(0.85f,0.78f,0.45f));

            // Roads
            CreateRoadStraight("Road_Straight_2m_W4", 4f, 2f, roadThickness, matRoad);
            CreateRoadStraight("Road_Straight_2m_W8", 8f, 2f, roadThickness, matRoad);

            // Buildings (shells)
            CreateBuildingShell("Building_Guild_16x14", 16f, 14f, 8f, matBld);
            CreateBuildingShell("Building_Market_12x10", 12f, 10f, 6f, matBld);
            CreateBuildingShell("Building_Inn_12x12", 12f, 12f, 6.5f, matBld);

            // POIs
            CreateColumn("Landmark_Statue_2m", 0.8f, 2f, matLM);
            CreateFountain("Landmark_Fountain_4m", 4f, 0.5f, matLM);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Starter Prefabs", "สร้าง Prefab เรียบร้อยแล้วใน " + baseFolder, "OK");
        }

        private void EnsureFolderRecursive(string path){
            path = path.Replace('\\','/');
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++){
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)){
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private Material CreateFlatMaterial(string name, Color color){
            EnsureFolderRecursive("Assets/Art/Materials");
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            AssetDatabase.CreateAsset(mat, $"Assets/Art/Materials/{name}.mat");
            return mat;
        }

        private void CreateRoadStraight(string prefabName, float width, float length, float thickness, Material mat){
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = prefabName;
            go.transform.localScale = new Vector3(width, thickness, length);
            var mr = go.GetComponent<MeshRenderer>();
            if (mr && mat) mr.sharedMaterial = mat;
            var col = go.GetComponent<BoxCollider>();
            if (col){ col.size = new Vector3(1f, 1f, 1f); col.center = Vector3.zero; }
            SavePrefab(go, $"{baseFolder}/Roads/{prefabName}.prefab");
        }

        private void CreateBuildingShell(string prefabName, float sizeX, float sizeZ, float height, Material mat){
            var root = new GameObject(prefabName);
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Shell";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(sizeX, height, sizeZ);
            var mr = body.GetComponent<MeshRenderer>();
            if (mr && mat) mr.sharedMaterial = mat;
            var box = body.GetComponent<BoxCollider>();
            if (box) { box.size = new Vector3(1f, 1f, 1f); box.center = Vector3.zero; }

            // Simple doorway notch (visual only)
            var notch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            notch.name = "Doorway";
            notch.transform.SetParent(root.transform, false);
            notch.transform.localScale = new Vector3(Mathf.Min(3f, sizeX*0.3f), Mathf.Min(3f, height*0.5f), 0.2f);
            notch.transform.localPosition = new Vector3(0f, notch.transform.localScale.y*0.5f - height*0.5f + 0.1f, sizeZ*0.5f - 0.11f);
            Object.DestroyImmediate(notch.GetComponent<BoxCollider>());

            SavePrefab(root, $"{baseFolder}/Buildings/{prefabName}.prefab");
        }

        private void CreateColumn(string prefabName, float radius, float height, Material mat){
            var root = new GameObject(prefabName);
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = "Column";
            cyl.transform.SetParent(root.transform, false);
            cyl.transform.localScale = new Vector3(radius, height*0.5f, radius);
            var mr = cyl.GetComponent<MeshRenderer>();
            if (mr && mat) mr.sharedMaterial = mat;
            SavePrefab(root, $"{baseFolder}/POIs/{prefabName}.prefab");
        }

        private void CreateFountain(string prefabName, float diameter, float rimHeight, Material mat){
            var root = new GameObject(prefabName);
            var baseBowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseBowl.name = "Basin";
            baseBowl.transform.SetParent(root.transform, false);
            baseBowl.transform.localScale = new Vector3(diameter, rimHeight, diameter);
            var mr = baseBowl.GetComponent<MeshRenderer>();
            if (mr && mat) mr.sharedMaterial = mat;
            SavePrefab(root, $"{baseFolder}/POIs/{prefabName}.prefab");
        }

        private void SavePrefab(GameObject temp, string path){
            EnsureFolderRecursive(Path.GetDirectoryName(path).Replace('\\','/'));
            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            Object.DestroyImmediate(temp);
        }
    }
}
#endif
