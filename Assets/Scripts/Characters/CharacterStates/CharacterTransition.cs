using Characters;
using Dev.Scripts.Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/Transition",fileName = "Transition")]
    public class CharacterTransition:StateData
    {
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            animator.SetBool(TransitionParameter.JumpEnd.ToString(),false);
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            SetTransition();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            animator.SetBool(TransitionParameter.ForceTransition.ToString(),false);
            animator.SetBool(TransitionParameter.JumpEnd.ToString(),false);
        }
        
        private void SetTransition()
        {
            if (CharacterInputController.SwipeUp)
            {
                _characterMovement.PlayAnim(TransitionParameter.Jumping.ToString(),0.01f,1f);
            }
            
            if (CharacterInputController.SwipeDown)
            {
                _characterMovement.PlayAnim(TransitionParameter.Sliding.ToString(),0.01f,1f);
            }
        }
    }
}