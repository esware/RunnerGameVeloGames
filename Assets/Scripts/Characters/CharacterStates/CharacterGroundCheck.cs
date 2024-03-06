using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/GroundCheck",fileName = "GroundCheck")]
    public class CharacterGroundCheck:StateData
    {
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            if (!_characterMovement)
            {
                Debug.LogError($"Character movement is null, gravity control cannot be performed in the ground check state. Current State: {stateInfo}");
                return;
            }

            _characterMovement.GroundCheck();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }
}