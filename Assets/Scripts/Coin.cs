using System;
using System.Collections;
using System.Numerics;
using Dev.Scripts.Obstacles;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Dev.Scripts
{
    public class Coin : MonoBehaviour
    {
        public static Pooler coinPool;
        private const int LayerMask = 1<<3;

        public float radius;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask)
            {
                coinPool.Free(this.gameObject);
            }
        }
        Obstacle obstacle = null;
        Collider[] colliders = new Collider[20];

        private void Start()
        {
            //StartCoroutine(CheckObstacle());
        }

        private IEnumerator CheckObstacle()
        {
            while (true)
            {
                while (Physics.OverlapSphereNonAlloc(transform.position, 1f,colliders, 1 << 9) > 0)
                {
                    Debug.Log("Engel");
                    foreach (var c in colliders)
                    {
                        if (c.GetComponent<Obstacle>() || c.GetComponentInParent<Obstacle>())
                        {
                            obstacle = c.gameObject.GetComponent<Obstacle>() ?? c.gameObject.GetComponentInParent<Obstacle>();
                            break;
                        }
                    }

                    if (obstacle != null)
                    {
                        if (obstacle.coinSpawnType == ObstacleCoinSpawnType.SpawnByJumping)
                        {
                            Debug.Log("Jumping");
                            break;
                        }
                        if (obstacle.coinSpawnType == ObstacleCoinSpawnType.DontSpawn)
                        {
                            Debug.Log("Dont Spawn");
                            Debug.Log("Lane Degistir");
                        }
                    }
                    yield return null;
                }

                yield return new WaitForSeconds(0.5f);
            }
            
        }
    }
}