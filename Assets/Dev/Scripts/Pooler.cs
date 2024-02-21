using System.Collections.Generic;
using UnityEngine;

namespace Dev.Scripts
{
    public class Pooler
    {
        private Stack<GameObject> m_FreeInstances = new Stack<GameObject>();
        private GameObject m_Original;

        public Pooler(GameObject original, int initialSize)
        {
            m_Original = original;
            m_FreeInstances = new Stack<GameObject>(initialSize);

            for (int i = 0; i < initialSize; ++i)
            {
                GameObject obj = Object.Instantiate(original);
                obj.SetActive(false);
                m_FreeInstances.Push(obj);
            }
        }

        public GameObject Get()
        {
            return Get(Vector3.zero, Quaternion.identity,false);
        }

        public GameObject Get(Vector3 pos, Quaternion quat,bool value)
        {
            GameObject ret = m_FreeInstances.Count > 0 ? m_FreeInstances.Pop() : Object.Instantiate(m_Original);

            ret.GetComponent<Coin>().isNegative = value;
            ret.SetActive(true);
            ret.transform.position = pos;
            ret.transform.rotation = quat;

            return ret;
        }

        public void Free(GameObject obj)
        {
            obj.transform.SetParent(null);
            obj.SetActive(false);
            m_FreeInstances.Push(obj);
        }
    }
}