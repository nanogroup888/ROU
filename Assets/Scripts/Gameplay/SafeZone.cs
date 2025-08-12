// ------------------------------------------------
// File: Assets/Scripts/Gameplay/SafeZone.cs
// ------------------------------------------------
using UnityEngine;

namespace ROLike.Gameplay {
    [RequireComponent(typeof(SphereCollider))]
    public class SafeZone : MonoBehaviour {
        public float radius = 20f;

        private void Reset(){
            var sc = GetComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = radius;
        }

        private void OnValidate(){
            var sc = GetComponent<SphereCollider>();
            if (sc){ sc.isTrigger = true; sc.radius = radius; }
        }

        private void OnDrawGizmosSelected(){
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.25f);
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 1f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
