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
        
       /* private void Update()
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, Vector3.forward, out hitInfo, 5, 1 << 9))
            {
                Debug.Log("Collided");
                Debug.DrawRay(transform.position, Vector3.forward,Color.green);
            }
            
            Debug.DrawRay(transform.position, Vector3.forward,Color.red);
        }*/
    }
}