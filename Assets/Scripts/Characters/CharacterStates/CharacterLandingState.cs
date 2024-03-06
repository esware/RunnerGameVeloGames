using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/JumpEndState",fileName = "JumpEndState")]
    public class CharacterLandingState : StateData
    {
        private CharacterMovement _characterMovement;

        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);

            if (_characterMovement == null)
            {
                Debug.LogError("CharacterMovement component could not be found!");
            }
            else
            {
                animator.SetBool(TransitionParameter.JumpEnd.ToString(), false);
            }
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (_characterMovement == null)
            {
                Debug.LogError("CharacterMovement component is null during update!");
                return;
            }
            _characterMovement.Move();
            
            if (!_characterMovement.isGrounded && _characterMovement.IsCloseToGround(0.5f))
            {
                animator.SetBool(TransitionParameter.JumpEnd.ToString(), true);
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            animator.SetBool(TransitionParameter.JumpEnd.ToString(), false);
        }
    }

}