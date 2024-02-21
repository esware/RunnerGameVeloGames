using Characters;
using Dev.Scripts.Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(fileName = "TransitionStates",menuName = "EWGames/CharacterStates/Transiton")]
    public class TransitionState:StateData
    {
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            SetTransition();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }

        private void SetTransition()
        {
            if (CharacterInputController.SwipeDown)
            {
                _characterMovement.PlayAnim(TransitionParameter.Sliding.ToString(),0.1f,1f);
            }
            if (CharacterInputController.SwipeUp)
            {
                _characterMovement.PlayAnim(TransitionParameter.Jumping.ToString(),0.1f,1f);
            }
        }
    }
}