using Characters;
using Dev.Scripts.Camera;
using Dev.Scripts.Characters;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/HitState",fileName = "NewState")]
    public class CharacterHitState : StateData
    {
        [Range(0, 1)]
        [SerializeField] private float transitionTiming;
    
        private CharacterMovement _characterMovement;
    
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            CharacterInputController.Instance.GetInputs = false;
            
            if (_characterMovement != null && _characterMovement.cameraShake != null)
            {
                _characterMovement.cameraShake.Shake(0.5f);
            }
            else
            {
                Debug.LogError("CharacterMovement or CameraShake component could not be found!");
            }
            
            if (_characterMovement != null && _characterMovement.trackManager.characterController != null)
            {
                var characterControl = _characterMovement.trackManager.characterController;
                characterControl.StopMoving();
                characterControl.CleanConsumable();
            }
            else
            {
                Debug.LogError("CharacterMovement or CharacterControl component could not be found!");
            }
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= transitionTiming)
            {
                _characterMovement.trackManager.characterController.StartRunning();
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            CharacterInputController.Instance.GetInputs = true;
        }
    }

}