using Characters;
using Dev.Scripts.Camera;
using Dev.Scripts.Track;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/IdleState",fileName = "NewState")]
    public class CharacterIdleState:StateData
    {
        private CharacterMovement _characterControl;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterControl = characterState.GetCharacterMovement(animator);
            _characterControl.cameraController.ChangeState(CameraStates.PlayerCam.ToString());
            _characterControl.trackManager.characterInputController.GetInputs = true;
        }

        public override void UpdateAbility(BaseState characterState, Animator animator,AnimatorStateInfo stateInfo)
        {
            
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }
}