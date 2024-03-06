using Cinemachine;
using UnityEngine;

namespace Dev.Scripts.Camera
{
    public class CameraShake : MonoBehaviour
    {
        #region Private Variables
    
        private CinemachineVirtualCamera _virtualCamera;
        private CinemachineBasicMultiChannelPerlin _noiseModule;
        private float _shakeDuration;
    
        #endregion

        #region Unity Callbacks

        void Start()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
            _noiseModule = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        void Update()
        {
            _shakeDuration = Mathf.Max(_shakeDuration - Time.deltaTime, 0);
        
            if (_shakeDuration == 0)
            {
                _noiseModule.m_AmplitudeGain = 0;
                _noiseModule.m_FrequencyGain = 0;
            }
        }

        #endregion

        #region Public Methods

        public void Shake(float duration)
        {
            _noiseModule.m_AmplitudeGain = 0.5f;
            _noiseModule.m_FrequencyGain = 1.5f;
            _shakeDuration = duration;
        }

        #endregion
    }

}