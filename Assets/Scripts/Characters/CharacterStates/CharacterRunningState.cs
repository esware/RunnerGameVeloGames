using Characters;
using Dev.Scripts.Characters;
using Dev.Scripts.Camera;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/RunningState",fileName = "NewState")]
    public class CharacterRunningState:StateData
    {
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            _characterMovement.controller.height =2f;
            _characterMovement.controller.center = new Vector3(0,1,0);
        }

        public override void UpdateAbility(BaseState characterState, Animator animator,AnimatorStateInfo stateInfo)
        {
            _characterMovement.Move();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            animator.SetBool(TransitionParameter.ForceTransition.ToString(),false);
        }
    }
}