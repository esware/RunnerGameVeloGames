﻿using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/JumpState",fileName = "NewState")]
    public class CharacterJumpState:StateData
    {
        [Range(0,1f)]
        [SerializeField] private float transitionTiming;
        private CharacterMovement _characterMovement;

        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            _characterMovement.Jump();
        }
        public override void UpdateAbility(BaseState characterState, Animator animator,AnimatorStateInfo stateInfo)
        {
            _characterMovement.Move();
            if (stateInfo.normalizedTime >= transitionTiming)
            {
                animator.SetBool("ForceTransition",true);
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            animator.SetBool("ForceTransition",false);
        }
        
        
    }
}