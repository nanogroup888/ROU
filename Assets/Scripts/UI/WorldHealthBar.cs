using UnityEngine;
using UnityEngine.UI;

namespace ROLikeMMO.UI
{
    public class WorldHealthBar : MonoBehaviour
    {
        public ROLikeMMO.Gameplay.Health health;
        public Transform follow;
        public Vector3 offset = new Vector3(0, 2.0f, 0);
        public Slider slider;
        public Camera cam;

        void Awake()
        {
            if (!cam) cam = Camera.main;
            if (!health) health = GetComponentInParent<ROLikeMMO.Gameplay.Health>();
        }

        void OnEnable()
        {
            if (health) health.onChanged.AddListener(OnHpChanged);
            RefreshImmediate();
        }

        void OnDisable()
        {
            if (health) health.onChanged.RemoveListener(OnHpChanged);
        }

        void LateUpdate()
        {
            if (!cam) cam = Camera.main;
            if (follow)
            {
                transform.position = follow.position + offset;
                if (cam) transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
            }
        }

        void OnHpChanged(int cur, int max)
        {
            if (slider)
            {
                slider.maxValue = max;
                slider.value = cur;
            }
        }

        public void RefreshImmediate()
        {
            if (health && slider)
            {
                slider.maxValue = health.max;
                slider.value = health.current;
            }
        }
    }
}
