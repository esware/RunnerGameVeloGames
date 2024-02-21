using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/SlideState",fileName = "Sliding")]
    public class CharacterSlidingState:StateData
    {
        [Range(0,1f)]
        [SerializeField] private float transitionTiming;

        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            _characterMovement.gravity *= 2;
            _characterMovement.controller.center =new Vector3(0,0.5f,0);
            _characterMovement.controller.height =1f;
            animator.SetBool(TransitionParameter.ForceTransition.ToString(),false);
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement.Move();
            if (stateInfo.normalizedTime >= transitionTiming)
            {
                animator.SetBool(TransitionParameter.ForceTransition.ToString(),true);
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement.gravity = -20;
            _characterMovement.controller.center =new Vector3(0,1,0);
            _characterMovement.controller.height =2f;
        }
    }
}