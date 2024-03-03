using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dev.Scripts.Consumables
{
    public abstract class Consumable : MonoBehaviour
    {
        public enum ConsumableType
        {
            NONE,
            COIN_MAG,
            EXTRALIFE,
        }
        
        public float duration;
        public AudioClip activatedSound;
        public AssetReference activatedParticleReference;
        public bool canBeSpawned = true;

        public bool active => _active;
        public float timeActive => _sinceStart;

        private bool _active = true;
        private float _sinceStart;
        private ParticleSystem _particleSpawned;

        public abstract ConsumableType GetConsumableType();
        public abstract string GetConsumableName();

        public void ResetTime()
        {
            _sinceStart = 0;
        }

        public virtual bool CanBeUsed(CharacterControl c)
        {
            return true;
        }

        public virtual IEnumerator Started(CharacterControl c)
        {
            _sinceStart = 0;

             if (activatedSound != null)
             {
                 c.powerupSource.clip = activatedSound;
                 c.powerupSource.Play();
                 c.powerupSource.volume = 0.3f;
             }

            if (activatedParticleReference != null)
            {
                var op = activatedParticleReference.InstantiateAsync();
                yield return op;
                _particleSpawned = op.Result.GetComponent<ParticleSystem>();
                if (!_particleSpawned.main.loop)
                    CoroutineHandler.StartStaticCoroutine(TimedRelease(_particleSpawned.gameObject,
                        duration));

                _particleSpawned.transform.SetParent(c.transform);
                _particleSpawned.transform.localPosition = new Vector3(0, 1, 0);
            }
        }

        IEnumerator TimedRelease(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            Addressables.ReleaseInstance(obj);
        }

        public virtual void Tick(CharacterControl c)
        {
            _sinceStart += Time.deltaTime;
            if (_sinceStart >= duration)
            {
                _active = false;
                return;
            }
        }

        public virtual void Ended(CharacterControl c)
        {
            if (_particleSpawned != null)
            {
                Addressables.ReleaseInstance(_particleSpawned.gameObject);
            }

            if (activatedSound != null && c.powerupSource.clip == activatedSound)
                c.powerupSource.Stop();

            for (int i = 0; i < c.consumables.Count; ++i)
            {
                if (c.consumables[i].active && c.consumables[i].activatedSound != null)
                {
                   
                    c.powerupSource.clip = c.consumables[i].activatedSound;
                    //c.powerupSource.Play();
                }
            }
        }
    }
}