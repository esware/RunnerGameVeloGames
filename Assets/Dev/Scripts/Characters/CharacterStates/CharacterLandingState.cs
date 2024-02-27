using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/JumpEndState",fileName = "JumpEndState")]
    public class CharacterLandingState:StateData
    {
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            animator.SetBool(TransitionParameter.JumpEnd.ToString(),false);
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (!_characterMovement.isGrounded)
            {
                if (_characterMovement.IsCloseToGround(.6f))
                {
                    animator.SetBool(TransitionParameter.JumpEnd.ToString(),true);
                }
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            animator.SetBool(TransitionParameter.JumpEnd.ToString(),false);
        }
    }
}