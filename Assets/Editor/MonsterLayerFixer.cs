#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ROLikeMMO.Tools
{
    public static class MonsterLayerFixer
    {
        [MenuItem("ROLike Tools/Fix Selected To Monster Layer")]
        public static void FixLayer()
        {
            int monsterLayer = LayerMask.NameToLayer("Monster");
            if (monsterLayer < 0)
            {
                EditorUtility.DisplayDialog("Layer Missing", "Please create a Layer named 'Monster' first (Edit > Project Settings > Tags and Layers).", "OK");
                return;
            }

            foreach (var obj in Selection.gameObjects)
            {
                SetLayerRecursively(obj, monsterLayer);
                var col = obj.GetComponent<Collider>();
                if (col == null) obj.AddComponent<CapsuleCollider>();
            }
            Debug.Log("[MonsterLayerFixer] Set layer to 'Monster' and ensured Collider on selected objects.");
        }

        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
                SetLayerRecursively(t.gameObject, layer);
        }
    }
}
#endif
