﻿using System.Collections.Generic;
using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    public class BaseState : StateMachineBehaviour
    {
        #region Variables

        public List<StateData> stateData = new List<StateData>();
        private CharacterMovement _characterMovement;
        private CharacterControl _characterControl;

        #endregion

        #region Getters

        public CharacterControl GetCharacterControl(Animator animator)
        {
            if (_characterControl == null)
            {
                _characterControl = animator.GetComponentInParent<CharacterControl>();
            }

            return _characterControl;
        }

        public CharacterMovement GetCharacterMovement(Animator animator)
        {
            if (_characterMovement == null)
            {
                _characterMovement = animator.GetComponentInParent<CharacterMovement>();
            }

            return _characterMovement;
        }

        #endregion

        #region Helper Methods

        private void UpdateAll(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            foreach (var d in stateData)
            {
                d.UpdateAbility(characterState, animator, stateInfo);
            }
        }

        #endregion

        #region StateMachineBehaviour Callbacks

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach(StateData d in stateData)
            {
                d.OnEnter(this, animator, stateInfo);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateAll(this,animator,stateInfo);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            foreach (StateData d in stateData)
            {
                d.OnExit(this, animator, stateInfo);
            }
        }

        #endregion
    }

}