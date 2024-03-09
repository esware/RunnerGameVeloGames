﻿using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/JumpEndState",fileName = "JumpEndState")]
    public class CharacterJumpEndState : StateData
    {
        [Range(0, 1)]
        [SerializeField] private float transitionTiming;

        private CharacterMovement _characterMovement;

        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);

            if (_characterMovement == null)
            {
                Debug.LogError("CharacterMovement component could not be found!");
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
            if (_characterMovement.isGrounded && stateInfo.normalizedTime >= transitionTiming)
            {
                _characterMovement.PlayAnim(TransitionParameter.Running.ToString(), 0.1f, 1f);
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }

}