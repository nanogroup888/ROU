using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace ROLikeMMO.UI
{
    /// <summary>
    /// Binds the local player's Health to a UI Slider on the HUD.
    /// Put this on a Canvas in the scene and assign hpSlider.
    /// </summary>
    public class PlayerHUDBinder : MonoBehaviour
    {
        public Slider hpSlider;

        void Start() => StartCoroutine(Bind());

        IEnumerator Bind()
        {
            while (NetworkClient.localPlayer == null) yield return null;
            var health = NetworkClient.localPlayer.GetComponent<ROLikeMMO.Gameplay.Health>();
            if (health == null) yield break;

            if (hpSlider != null)
            {
                hpSlider.maxValue = health.max;
                hpSlider.value = health.current;
                health.OnChanged += OnChanged;
            }
        }

        void OnChanged(int cur, int max)
        {
            if (hpSlider == null) return;
            hpSlider.maxValue = max;
            hpSlider.value = cur;
        }

        void OnDestroy()
        {
            if (NetworkClient.localPlayer == null) return;
            var health = NetworkClient.localPlayer.GetComponent<ROLikeMMO.Gameplay.Health>();
            if (health != null) health.OnChanged -= OnChanged;
        }
        }
}
