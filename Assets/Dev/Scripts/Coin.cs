using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Dev.Scripts
{
    public class Coin : MonoBehaviour
    {
        public static Pooler coinPool;
        private const int LayerMask = 9;

        public float maxDistance;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask)
            {
                coinPool.Free(this.gameObject);
            }
        }

        void Update()
        {
            DrawSphereCast(transform.position,Vector3.up,2.5f,maxDistance,Color.blue);
            RaycastHit hit;
            if (Physics.SphereCast(transform.position,2.5f, Vector3.up,out hit,maxDistance,9,QueryTriggerInteraction.UseGlobal ))
            {
                Debug.Log("Objeye carpti"+hit.collider.gameObject.name);
            }
        }
        
        void DrawSphereCast(Vector3 origin, Vector3 direction, float radius, float distance, Color color)
        {
            RaycastHit hit;
            bool isHit = Physics.SphereCast(origin, radius, direction, out hit, distance, 1 << 9);

            if (isHit)
            {
                var center = transform.position;
                Debug.DrawLine(origin, hit.point, Color.red); 
                Debug.DrawRay(hit.point, hit.normal, Color.blue); 
                Debug.DrawLine(center + new Vector3(-radius, 0, 0), center + new Vector3(radius, 0, 0), color);
                Debug.DrawLine(center + new Vector3(0, -radius, 0), center + new Vector3(0, radius, 0), color);
                // Draw vertical lines
                Debug.DrawLine(center + new Vector3(-radius, 0, 0), center + new Vector3(-radius, 0, 0) + Vector3.up * radius * 2, color);
                Debug.DrawLine(center + new Vector3(radius, 0, 0), center + new Vector3(radius, 0, 0) + Vector3.up * radius * 2, color);
                // Draw diagonal lines
                Debug.DrawLine(center + new Vector3(0, 0, -radius), center + new Vector3(0, 0, radius), color);
                Debug.DrawLine(center + new Vector3(-radius, 0, 0) + Vector3.up * radius * 2, center + new Vector3(radius, 0, 0) + Vector3.up * radius * 2, color);
            }
            else
            {
                Debug.DrawRay(origin, direction * distance, Color.green);
            }
        }

    }
}