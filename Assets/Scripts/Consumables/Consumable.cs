using System.Collections;
using System.Collections.Generic;
using Dev.Scripts.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Dev.Scripts.Consumables
{
    public abstract class Consumable : MonoBehaviour
    {
        public float duration;
        [HideInInspector] public PowerupIcon powerupIcon;

        public enum ConsumableType
        {
            NONE,
            COIN_MAG,
            EXTRALIFE,
        }

        public Sprite icon;
        public AudioClip activatedSound;
        public AssetReference ActivatedParticleReference;
        public bool canBeSpawned = true;

        public bool active
        {
            get { return m_Active; }
        }

        public float timeActive
        {
            get { return m_SinceStart; }
        }

        protected bool m_Active = true;
        protected float m_SinceStart;
        protected ParticleSystem m_ParticleSpawned;

        public abstract ConsumableType GetConsumableType();
        public abstract string GetConsumableName();

        public void ResetTime()
        {
            m_SinceStart = 0;
        }

        public virtual bool CanBeUsed(CharacterControl c)
        {
            return true;
        }

        public virtual IEnumerator Started(CharacterControl c)
        {
            m_SinceStart = 0;

             if (activatedSound != null)
             {
                 c.powerupSource.clip = activatedSound;
                 c.powerupSource.Play();
                 c.powerupSource.volume = 0.3f;
             }

            if (ActivatedParticleReference != null)
            {
                var op = ActivatedParticleReference.InstantiateAsync();
                yield return op;
                m_ParticleSpawned = op.Result.GetComponent<ParticleSystem>();
                if (!m_ParticleSpawned.main.loop)
                    CoroutineHandler.StartStaticCoroutine(TimedRelease(m_ParticleSpawned.gameObject,
                        duration));

                m_ParticleSpawned.transform.SetParent(c.transform);
                m_ParticleSpawned.transform.localPosition = new Vector3(0, 1, 0);
            }
        }

        IEnumerator TimedRelease(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            Addressables.ReleaseInstance(obj);
        }

        public virtual void Tick(CharacterControl c)
        {
            m_SinceStart += Time.deltaTime;
            if (m_SinceStart >= duration)
            {
                m_Active = false;
                return;
            }
        }

        public virtual void Ended(CharacterControl c)
        {
            if (m_ParticleSpawned != null)
            {
                Addressables.ReleaseInstance(m_ParticleSpawned.gameObject);
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