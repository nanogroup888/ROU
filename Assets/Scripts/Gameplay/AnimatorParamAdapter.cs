using UnityEngine;

namespace ROLikeMMO.Gameplay
{
    /// <summary>
    /// Bridges our standard parameters to the vendor's Animator parameter names.
    /// Put this on the same object that has the Animator (usually the mesh).
    /// </summary>
    public class AnimatorParamAdapter : MonoBehaviour
    {
        public Animator animator;

        [Header("Map our params -> Vendor params")]
        public string speedParam = "Speed";     // vendor's float for movement
        public string attackTrigger = "Attack"; // vendor's attack trigger
        public string hitTrigger = "Hit";       // vendor's hit trigger
        public string dieTrigger = "Die";       // vendor's die trigger

        void Reset() { animator = GetComponent<Animator>(); }

        public void SetSpeed(float v) { if (animator) animator.SetFloat(speedParam, v); }
        public void PlayAttack() { if (animator) animator.SetTrigger(attackTrigger); }
        public void PlayHit() { if (animator) animator.SetTrigger(hitTrigger); }
        public void PlayDie() { if (animator) animator.SetTrigger(dieTrigger); }
    }
}
