﻿using System.Collections.Generic;
using UnityEngine;

namespace Dev.Scripts
{
    public class Pooler
    {
        private readonly Stack<GameObject> _freeInstances;
        private readonly GameObject _original;
        
        public Pooler(GameObject original, int initialSize)
        {
            _original = original;
            _freeInstances = new Stack<GameObject>(initialSize);

            for (int i = 0; i < initialSize; ++i)
            {
                GameObject obj = Object.Instantiate(original);
                obj.SetActive(false);
                _freeInstances.Push(obj);
            }
        }

        public GameObject Get()
        {
            return Get(Vector3.zero, Quaternion.identity);
        }

        public GameObject Get(Vector3 pos, Quaternion quat)
        {
            GameObject ret = _freeInstances.Count > 0 ? _freeInstances.Pop() : Object.Instantiate(_original);
            
            ret.SetActive(true);
            ret.transform.position = pos;
            ret.transform.rotation = quat;

            return ret;
        }

        public void Free(GameObject obj)
        {
            obj.transform.SetParent(null);
            obj.SetActive(false);
            _freeInstances.Push(obj);
        }
    }
}