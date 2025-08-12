using UnityEngine;
using TMPro;

namespace ROLikeMMO.UI
{
    public class DamagePopupManager : MonoBehaviour
    {
        public static DamagePopupManager Instance { get; private set; }

        [Header("References")]
        public Canvas canvas;                 // ใส่ Canvas ที่จะวาง popup (Screen Space หรือ World Space ก็ได้)
        public RectTransform container;       // วาง popup ใต้ตัวนี้ (ปล่อยว่าง = ใช้ canvas.transform)
        public DamagePopup popupPrefab;       // พรีแฟบ Text (UI) + DamagePopup.cs

        Camera uiCamera;                      // กล้องใช้แปลงพิกัด (ขึ้นกับ Render Mode)
        RectTransform canvasRT;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            canvasRT = canvas ? canvas.GetComponent<RectTransform>() : null;

            // กล้องที่ใช้คำนวณตำแหน่งบน Canvas
            uiCamera = canvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null :
                       (canvas && canvas.worldCamera != null ? canvas.worldCamera : Camera.main);

            if (container == null && canvas != null) container = canvas.transform as RectTransform;
        }

        /// <summary>แสดงดาเมจจากตำแหน่ง "โลก 3D"</summary>
        public void ShowDamageWorld(Vector3 worldPos, int amount, bool crit = false, float verticalOffset = 1.2f)
        {
            if (!canvasRT || !popupPrefab) return;

            // ยกตำแหน่งขึ้นเล็กน้อยจากจุดโดน
            worldPos += Vector3.up * verticalOffset;

            // World -> Screen
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

            // Screen -> Local Canvas
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screen, uiCamera, out local))
                return;

            // สร้าง popup ใต้ container
            var popup = Instantiate(popupPrefab, container ? container : canvasRT);
            var rt = popup.transform as RectTransform;
            rt.anchoredPosition = local;
            rt.rotation = Quaternion.identity;
            rt.localScale = Vector3.one;

            popup.Setup(amount, crit);
        }

        /// <summary>แสดงดาเมจจากพิกัดหน้าจอ (ถ้ามีอยู่แล้ว)</summary>
        public void ShowDamageScreen(Vector2 screenPos, int amount, bool crit = false)
        {
            if (!canvasRT || !popupPrefab) return;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, uiCamera, out local))
                return;

            var popup = Instantiate(popupPrefab, container ? container : canvasRT);
            var rt = popup.transform as RectTransform;
            rt.anchoredPosition = local;
            rt.rotation = Quaternion.identity;
            rt.localScale = Vector3.one;

            popup.Setup(amount, crit);
        }
    }
}
