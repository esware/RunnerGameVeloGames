using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/JumpState",fileName = "NewState")]
    public class CharacterJumpState : StateData
    {
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

            if (!_characterMovement.isGrounded)
                return;
            
            _characterMovement.Jump();
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }

}