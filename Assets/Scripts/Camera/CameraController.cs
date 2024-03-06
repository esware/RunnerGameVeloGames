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
        #region Private Variables

        private Animator _animator;

        #endregion
        
        #region Unity Callbacks
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        #endregion

        #region Public Methods

        public void ChangeState(string states)
        {
            _animator.Play(states);
        }
        
        #endregion
        
    }
}