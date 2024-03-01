using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Dev.Scripts
{
    public class Coin : MonoBehaviour
    {
        public static Pooler coinPool;
        private const int LayerMask = 1<<3;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask)
            {
                coinPool.Free(this.gameObject);
            }
        }
    }
}