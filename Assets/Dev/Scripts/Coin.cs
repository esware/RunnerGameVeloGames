using UnityEngine;

namespace Dev.Scripts
{
    public class Coin : MonoBehaviour
    {
        static public Pooler coinPool;
        public bool isPremium = false;
        public bool isNegative = false;
        protected const int k_LayerMask = 9;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == k_LayerMask)
            {
                coinPool.Free(this.gameObject);
            }
        }
    }
}