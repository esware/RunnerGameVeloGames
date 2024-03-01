using System;
using System.Collections;
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

        private void Start()
        {
            //StartCoroutine(CollisionDetection());
        }

        private IEnumerator CollisionDetection()
        {
            while (true)
            {
                if (Physics.CheckSphere(transform.position,1f,1<<9))
                {
                    Debug.Log("Obstacle");
                }

                yield return null;
            }
        }
    }
}