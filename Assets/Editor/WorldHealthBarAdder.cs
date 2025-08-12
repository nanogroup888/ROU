#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace ROLikeMMO.Tools
{
    public static class WorldHealthBarAdder
    {
        [MenuItem("ROLike Tools/Add World HealthBar To Selected")]
        public static void AddBars()
        {
            foreach (var go in Selection.gameObjects)
            {
                AddBar(go);
            }
        }

        static void AddBar(GameObject root)
        {
            var anchor = root.transform.Find("HealthBarAnchor");
            if (anchor == null)
            {
                var a = new GameObject("HealthBarAnchor");
                a.transform.SetParent(root.transform, false);
                a.transform.localPosition = new Vector3(0, 1.8f, 0);
                anchor = a.transform;
            }

            var canvasGO = new GameObject("HPBar_Canvas");
            canvasGO.transform.SetParent(anchor, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
            canvasGO.AddComponent<GraphicRaycaster>();

            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1.6f, 0.2f);

            // background
            var bgGO = new GameObject("BG");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bg = bgGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            // slider
            var sliderGO = new GameObject("Slider");
            sliderGO.transform.SetParent(canvasGO.transform, false);
            var slider = sliderGO.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.transition = Selectable.Transition.None;
            slider.handleRect = null;

            var fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(sliderGO.transform, false);
            var faRT = fillArea.AddComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0.05f, 0.2f);
            faRT.anchorMax = new Vector2(0.95f, 0.8f);
            faRT.offsetMin = faRT.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.9f, 0.2f, 0.9f);
            var fillRT = fill.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0, 0);
            fillRT.anchorMax = new Vector2(1, 1);
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;

            slider.fillRect = fillRT;
            slider.targetGraphic = fillImg;

            // bind script
            var whb = canvasGO.AddComponent<ROLikeMMO.UI.WorldHealthBar>();
            var hp = root.GetComponent<ROLikeMMO.Gameplay.Health>();
            if (!hp) hp = root.AddComponent<ROLikeMMO.Gameplay.Health>();
            whb.health = hp;
            whb.follow = anchor;
            whb.slider = slider;

            // init
            whb.RefreshImmediate();

            Debug.Log($"[WorldHealthBarAdder] Added bar to {root.name}");
        }
    }
}
#endif
