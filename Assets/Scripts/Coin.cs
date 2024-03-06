using System;
using UnityEngine;

namespace Dev.Scripts
{
    public class Coin : MonoBehaviour
    {
        public static Pooler CoinPool;
        private const int LayerMask = 1<<3;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask)
            {
                CoinPool.Free(gameObject);
            }
        }
    }
}