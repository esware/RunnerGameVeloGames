using System.Collections;
using UnityEngine;

namespace Dev
{
    public class CoroutineHandler : MonoBehaviour
    {
        private static CoroutineHandler _instance;
        public static CoroutineHandler Instance
        {
            get
            {
                if(_instance == null)
                {
                    GameObject o = new GameObject("CoroutineHandler");
                    DontDestroyOnLoad(o);
                    _instance = o.AddComponent<CoroutineHandler>();
                }

                return _instance;
            }
        }

        public void OnDisable()
        {
            if(_instance)
                Destroy(_instance.gameObject);
        }

        public Coroutine StartStaticCoroutine(IEnumerator coroutine)
        {
            return Instance.StartCoroutine(coroutine);
        }
    }
}