using Characters;
using Dev.Scripts.Camera;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/HitState",fileName = "NewState")]
    public class CharacterHitState:StateData
    {
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            _characterMovement.cameraShake.Shake(0.5f);
            _characterMovement.cameraController.ChangeState(CameraStates.DeathCam.ToString());
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement.GroundCheck();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }
}