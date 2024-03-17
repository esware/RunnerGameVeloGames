using Characters;
using Dev.Scripts.Camera;
using Dev.Scripts.Track;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/IdleState",fileName = "NewState")]
    public class CharacterIdleState : StateData
    {
        private CharacterMovement _characterMovement;

        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            
            if (_characterMovement != null)
            {
                _characterMovement.cameraController.ChangeState(CameraStates.PlayerCam.ToString());
                
                if (_characterMovement.trackManager.characterInputController != null)
                {
                    _characterMovement.trackManager.characterInputController.GetInputs = true;
                }
                else
                {
                    Debug.LogError("CharacterInputController component could not be found!");
                }
            }
            else
            {
                Debug.LogError("CharacterMovement component could not be found!");
            }
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement.ApplyGravity();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }

}