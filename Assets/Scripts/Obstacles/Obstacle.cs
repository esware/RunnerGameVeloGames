using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Dev.Scripts.Obstacles
{
    public enum ObstacleCoinSpawnType
    {
        DontSpawn,
        SpawnByJumping,
    }

    [RequireComponent(typeof(AudioSource))]
    public abstract class Obstacle : MonoBehaviour
    {
        public ObstacleCoinSpawnType coinSpawnType;
        public AudioClip impactedSound;
        public bool randomColor;
        
        [ShowIf("randomColor")]
        public Color[] colors;

        public float obstacleLength;

        public abstract IEnumerator Spawn(TrackSegment segment, float t);

        public virtual void Impacted()
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            Animation anim = GetComponentInChildren<Animation>();
            
            if (anim != null)
            {
                anim.Play();
                float animationLength = anim.clip.length;
                StartCoroutine(DestroyAfterDelay(animationLength + 0.6f));
            }
            
            if (audioSource != null && impactedSound != null)
            {
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.clip = impactedSound;
                audioSource.Play();
            }
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Addressables.ReleaseInstance(gameObject);
        }
    }

}