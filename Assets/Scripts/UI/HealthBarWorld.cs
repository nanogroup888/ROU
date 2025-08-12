using UnityEngine;
using UnityEngine.UI;

namespace ROLikeMMO.UI
{
    /// <summary>
    /// World-space health bar for monsters. Attach to a Canvas (World Space) with a Slider.
    /// </summary>
    public class HealthBarWorld : MonoBehaviour
    {
        public ROLikeMMO.Gameplay.Health target;
        public Slider slider;
        public Vector3 offset = new Vector3(0, 1.6f, 0);

        Camera cam;

        void Start()
        {
            cam = Camera.main;
            if (target != null && slider != null)
            {
                slider.maxValue = target.max;
                slider.value = target.current;
                target.OnChanged += OnChanged;
            }
        }

        void LateUpdate()
        {
            if (target == null || slider == null) return;

            // keep above head
            transform.position = target.transform.position + offset;

            // face camera (billboard)
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                var dir = (transform.position - cam.transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        void OnChanged(int cur, int max)
        {
            slider.maxValue = max;
            slider.value = cur;
            slider.gameObject.SetActive(cur < max); // hide when full
        }

        void OnDestroy()
        {
            if (target != null) target.OnChanged -= OnChanged;
        }
    }
}
