﻿using Characters;
using Dev.Scripts.Camera;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/DeathState",fileName = "NewState")]
    public class CharacterDeathState:StateData
    {
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            _characterMovement.controller.height =2f;
            _characterMovement.controller.center = new Vector3(0,1,0);
            _characterMovement.cameraController.ChangeState(CameraStates.DeathCam.ToString());
            _characterMovement.trackManager.characterInputController.GetInputs = false;
            _characterMovement.cameraShake.Shake(0.5f);
        }

        public override void UpdateAbility(BaseState characterState, Animator animator,AnimatorStateInfo stateInfo)
        {
            _characterMovement.GroundCheck();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }
}