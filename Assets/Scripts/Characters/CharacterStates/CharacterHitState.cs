using Characters;
using Dev.Scripts.Camera;
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
            
            if (_characterMovement != null && _characterMovement.cameraShake != null)
            {
                _characterMovement.cameraShake.Shake(0.5f);
            }
            else
            {
                Debug.LogError("CharacterMovement or CameraShake component could not be found!");
            }
            
            if (_characterMovement != null && _characterMovement.characterControl != null)
            {
                _characterMovement.characterControl.StopMoving();
                _characterMovement.characterControl.CleanConsumable();
            }
            else
            {
                Debug.LogError("CharacterMovement or CharacterControl component could not be found!");
            }
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (stateInfo.normalizedTime >= transitionTiming && _characterMovement != null && _characterMovement.characterControl != null)
            {
                _characterMovement.characterControl.StartRunning();
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }

}