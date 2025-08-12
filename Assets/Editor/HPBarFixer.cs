#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.Linq;

namespace ROLikeMMO.Tools
{
    public static class HPBarFixer
    {
        const float WIDTH = 1.6f;
        const float HEIGHT = 0.2f;
        static readonly Vector3 SCALE = new Vector3(0.01f, 0.01f, 0.01f);
        static readonly Color BG_COLOR = new Color(0f, 0f, 0f, 0.5f);
        static readonly Color FILL_COLOR = new Color(0.2f, 0.9f, 0.2f, 0.9f);

        static Type worldHealthBarType;
        static Type WorldHealthBarType
        {
            get
            {
                if (worldHealthBarType == null)
                {
                    worldHealthBarType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                        .FirstOrDefault(t => t.FullName == "ROLikeMMO.UI.WorldHealthBar");
                }
                return worldHealthBarType;
            }
        }

        [MenuItem("ROLike Tools/Fix World HP Bars (Selected)")]
        public static void FixSelected()
        {
            var roots = Selection.gameObjects;
            if (roots == null || roots.Length == 0)
            {
                EditorUtility.DisplayDialog("Select something",
                    "Select the HPBar_Canvas or the root monster/player object and run again.\n\n" +
                    "Or use: ROLike Tools → Fix World HP Bars (Scene)", "OK");
                return;
            }

            int fixedCount = 0;
            foreach (var r in roots) fixedCount += FixAllUnder(r);
            EditorUtility.DisplayDialog("HP Bars Fixed", $"Fixed {fixedCount} HP bars under selection.", "OK");
        }

        [MenuItem("ROLike Tools/Fix World HP Bars (Scene)")]
        public static void FixScene()
        {
            var canvases = GameObject.FindObjectsOfType<Canvas>(true);
            int fixedCount = 0;
            foreach (var c in canvases)
                if (LooksLikeHPBar(c.gameObject))
                    fixedCount += FixSingle(c.gameObject);
            EditorUtility.DisplayDialog("HP Bars Fixed", $"Fixed {fixedCount} HP bars in scene.", "OK");
        }

        static int FixAllUnder(GameObject root)
        {
            int fixedCount = 0;
            var canvases = root.GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases)
                if (LooksLikeHPBar(c.gameObject))
                    fixedCount += FixSingle(c.gameObject);
            return fixedCount;
        }

        static bool LooksLikeHPBar(GameObject go)
        {
            bool named = go.name.ToLower().Contains("hpbar");
            bool hasSlider = go.GetComponentsInChildren<Slider>(true).Length > 0;
            bool hasWHB = (WorldHealthBarType != null) && (go.GetComponent(WorldHealthBarType) != null);
            return named || hasSlider || hasWHB;
        }

        static int FixSingle(GameObject canvasGO)
        {
            var canvas = canvasGO.GetComponent<Canvas>();
            if (!canvas) return 0;

            Undo.RegisterCompleteObjectUndo(canvasGO, "Fix HP Bar");

            // World-space
            canvas.renderMode = RenderMode.WorldSpace;
            if (!canvas.worldCamera) canvas.worldCamera = Camera.main;

            // size / scale
            var rt = canvas.GetComponent<RectTransform>();
            if (rt) Undo.RecordObject(rt, "Fix HP Bar");
            rt.sizeDelta = new Vector2(WIDTH, HEIGHT);
            canvasGO.transform.localScale = SCALE;

            // BG
            var bgTf = canvasGO.transform.Find("BG");
            Image bgImg;
            if (!bgTf)
            {
                var bgGO = new GameObject("BG");
                bgGO.transform.SetParent(canvasGO.transform, false);
                bgImg = bgGO.AddComponent<Image>();
            }
            else bgImg = bgTf.GetComponent<Image>() ?? bgTf.gameObject.AddComponent<Image>();

            if (bgImg) Undo.RecordObject(bgImg, "Fix HP Bar");
            var bgRT = bgImg.rectTransform;
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            bgImg.color = BG_COLOR;

            // Slider
            var slider = canvasGO.GetComponentInChildren<Slider>(true);
            if (!slider)
            {
                var sGO = new GameObject("Slider");
                sGO.transform.SetParent(canvasGO.transform, false);
                slider = sGO.AddComponent<Slider>();
                slider.transition = Selectable.Transition.None;
                slider.direction = Slider.Direction.LeftToRight;
            }
            Undo.RecordObject(slider, "Fix HP Bar");

            // FillArea
            RectTransform faRT;
            var fa = slider.transform.Find("FillArea");
            if (!fa)
            {
                var go = new GameObject("FillArea");
                go.transform.SetParent(slider.transform, false);
                faRT = go.AddComponent<RectTransform>();
            }
            else faRT = fa.GetComponent<RectTransform>();
            Undo.RecordObject(faRT, "Fix HP Bar");

            faRT.anchorMin = new Vector2(0.05f, 0.2f);
            faRT.anchorMax = new Vector2(0.95f, 0.8f);
            faRT.offsetMin = Vector2.zero;
            faRT.offsetMax = Vector2.zero;

            // Fill
            Image fillImg;
            RectTransform fillRT;
            var fill = faRT.transform.Find("Fill");
            if (!fill)
            {
                var go = new GameObject("Fill");
                go.transform.SetParent(faRT, false);
                fillImg = go.AddComponent<Image>();
                fillRT = go.GetComponent<RectTransform>();
            }
            else
            {
                fillImg = fill.GetComponent<Image>() ?? fill.gameObject.AddComponent<Image>();
                fillRT = fill.GetComponent<RectTransform>();
            }
            Undo.RecordObject(fillImg, "Fix HP Bar");
            fillImg.color = FILL_COLOR;

            slider.fillRect = fillRT;
            slider.handleRect = null;
            slider.targetGraphic = fillImg;

            // Bind to WorldHealthBar if present
            if (WorldHealthBarType != null)
            {
                var whb = canvasGO.GetComponent(WorldHealthBarType);
                if (whb != null)
                {
                    var sliderField = WorldHealthBarType.GetField("slider");
                    if (sliderField != null) sliderField.SetValue(whb, slider);

                    var camField = WorldHealthBarType.GetField("cam");
                    if (camField != null && camField.GetValue(whb) == null)
                        camField.SetValue(whb, Camera.main);
                }
            }

            EditorUtility.SetDirty(canvasGO);
            return 1;
        }
    }
}
#endif
