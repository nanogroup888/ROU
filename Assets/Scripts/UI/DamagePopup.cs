using UnityEngine;
using TMPro;

namespace ROLikeMMO.UI
{
    public class DamagePopup : MonoBehaviour
    {
        [Header("Motion Settings")]
        public float floatUpSpeed = 50f; // ความเร็วลอยขึ้น (หน่วยพิกเซล/วินาที)
        public float lifetime = 0.8f;
        public Vector2 randomOffset = new Vector2(30f, 15f); // กระจายตำแหน่งเล็กน้อย

        [Header("Animation Curves")]
        public AnimationCurve scaleOverLife = AnimationCurve.EaseInOut(0f, 1.2f, 1f, 0.8f);
        public AnimationCurve alphaOverLife = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        private TMP_Text tmpText;
        private float age;

        void Awake()
        {
            tmpText = GetComponent<TMP_Text>();

            // กระจายตำแหน่งเริ่มเล็กน้อย
            transform.localPosition += new Vector3(
                Random.Range(-randomOffset.x, randomOffset.x),
                Random.Range(-randomOffset.y, randomOffset.y),
                0f
            );
        }

        /// <summary>
        /// ตั้งค่าข้อความ/สี/คริติคอล
        /// </summary>
        public void Setup(int damage, bool crit = false)
        {
            tmpText.text = crit ? $"{damage}!" : damage.ToString();
            tmpText.fontStyle = crit ? FontStyles.Bold : FontStyles.Normal;
            tmpText.color = crit ? new Color(1f, 0.25f, 0.25f, 1f) : new Color(1f, 0.5f, 0.5f, 1f);

            if (crit)
                floatUpSpeed *= 1.15f;
        }

        void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / lifetime);

            // ลอยขึ้น
            transform.localPosition += Vector3.up * floatUpSpeed * Time.deltaTime;

            // ขยาย/ย่อขนาด
            float s = scaleOverLife.Evaluate(t);
            transform.localScale = Vector3.one * s;

            // จางหาย
            var c = tmpText.color;
            c.a = alphaOverLife.Evaluate(t);
            tmpText.color = c;

            // ลบเมื่อหมดเวลา
            if (age >= lifetime)
                Destroy(gameObject);
        }
    }
}
