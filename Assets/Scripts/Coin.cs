using System;
using Dev.Scripts.Obstacles;
using Dev.Scripts.Track;
using Unity.VisualScripting;
using UnityEngine;

namespace Dev.Scripts
{
    public class Coin : MonoBehaviour
    {
        [HideInInspector] public TrackManager trackManager;
        public static Pooler CoinPool;
        
        /*
        private const int LayerMask = 1<<3;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask)
            {
                
                trackManager.AddScore(1);
                CoinPool.Free(gameObject);
            }
        }*/
    }
}