using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dev.Scripts.Consumables
{
    public abstract class Consumable : MonoBehaviour
    {
        #region Enums

        public enum ConsumableType
        {
            None,
            CoinMag,
            Extralife,
        }

        #endregion

        #region Public Variables

        [Space,Header("Consumable Settings")]
        public float duration;
        public AudioClip activatedSound;
        public AssetReference activatedParticleReference;
        public bool canBeSpawned = true;

        #endregion
        
        #region Properties

        public bool IsActive => _isActive;
        public float TimeActive => _sinceStart;

        #endregion
        
        #region Private Variables

        private bool _isActive = true;
        private float _sinceStart;
        private ParticleSystem _particleSpawned;

        #endregion

        #region Public Methods

        public abstract ConsumableType GetConsumableType();
        public abstract string GetConsumableName();
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
                    CoroutineHandler.Instance.StartStaticCoroutine(TimedRelease(_particleSpawned.gameObject,
                        duration));

                Transform transform1;
                (transform1 = _particleSpawned.transform).SetParent(c.transform);
                transform1.localPosition = new Vector3(0, 1, 0);
            }
        }
        public virtual void Tick(CharacterControl c)
        {
            _sinceStart += Time.deltaTime;
            if (_sinceStart >= duration)
            {
                _isActive = false;
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

            for (int i = 0; i < c.Consumables.Count; ++i)
            {
                if (c.Consumables[i].IsActive && c.Consumables[i].activatedSound != null)
                {
                   
                    c.powerupSource.clip = c.Consumables[i].activatedSound;
                    c.powerupSource.Play();
                }
            }
        }
        public void ResetTime()
        {
            _sinceStart = 0;
        }

        #endregion

        #region Private Methods
        private IEnumerator TimedRelease(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            Addressables.ReleaseInstance(obj);
        }

        #endregion
  
    }
}