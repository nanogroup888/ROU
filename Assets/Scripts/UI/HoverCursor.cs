using UnityEngine;
using UnityEngine.EventSystems;

namespace ROLikeMMO.UI
{
    /// <summary>
    /// Change mouse cursor to a sword when hovering monsters (by LayerMask).
    /// Safe for both Scene/Game view. Client-side only.
    /// </summary>
    public class HoverCursor : MonoBehaviour
    {
        [Header("Cursor")]
        public Texture2D cursorSword;
        public Texture2D cursorDefault;
        public Vector2 swordHotspot = Vector2.zero;
        public float rayDistance = 100f;

        [Header("Targeting")]
        public LayerMask monsterMask; // tick 'Monster' layer

        Camera cam;
        bool showingSword;

        void Update()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                SetDefault(); // don't override UI cursor
                return;
            }

            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            int mask = monsterMask.value == 0 ? ~0 : monsterMask.value;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            bool overMonster = Physics.Raycast(ray, out _, rayDistance, mask);

            if (overMonster) SetSword();
            else SetDefault();
        }

        void SetSword()
        {
            if (showingSword) return;
            if (cursorSword != null)
            {
                Cursor.SetCursor(cursorSword, swordHotspot, CursorMode.Auto);
                showingSword = true;
            }
        }

        void SetDefault()
        {
            if (!showingSword) return;
            Cursor.SetCursor(cursorDefault, Vector2.zero, CursorMode.Auto);
            showingSword = false;
        }

        void OnDisable() => SetDefault();
    }
}
