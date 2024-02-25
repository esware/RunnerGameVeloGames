using UnityEngine;

namespace Dev.Scripts
{
    public class Coin : MonoBehaviour
    {
        public static Pooler coinPool;
        private const int LayerMask = 9;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask)
            {
                coinPool.Free(this.gameObject);
            }
        }
    }
}