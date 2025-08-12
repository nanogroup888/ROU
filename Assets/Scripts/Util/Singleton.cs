using UnityEngine;

namespace ROLikeMMO.Util
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
        protected virtual void Awake()
        {
            if (_instance == null) _instance = this as T;
            else if (_instance != this) Destroy(gameObject);
        }
    }
}
