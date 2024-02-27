using System.Collections;
using UnityEngine;

namespace Dev.Scripts.Camera
{
    public enum CameraStates
    {
        IdleCam,
        PlayerCam,
        DeathCam,
    }
    public class CameraController:MonoBehaviour
    {
        private Animator _animator;
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void ChangeState(string states)
        {
            _animator.Play(states);
        }
        
        public IEnumerator Shake(float duration, float magnitude)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0.0f;
            while (elapsed  <duration)
            {
                var x = Random.Range(-1f, 1f) * magnitude;
                var y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(originalPos.x+x, originalPos.y+y, originalPos.z);

                elapsed += Time.deltaTime;
                        
                yield return null;
            }

            transform.localPosition = originalPos;
            yield return new WaitForSeconds(0.2f);
        }
        
    }
}