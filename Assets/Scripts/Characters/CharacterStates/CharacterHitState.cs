using Characters;
using Dev.Scripts.Camera;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/HitState",fileName = "NewState")]
    public class CharacterHitState:StateData
    {
        [FormerlySerializedAs("TransitionTiming")]
        [Range(0, 1)]
        [SerializeField]
        private float transitionTiming;
        private CharacterMovement _characterMovement;
        public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement = characterState.GetCharacterMovement(animator);
            _characterMovement.cameraShake.Shake(0.5f);
            _characterMovement.characterControl.StopMoving();
            _characterMovement.characterControl.CleanConsumable();
        }

        public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            _characterMovement.GroundCheck();

            if (stateInfo.normalizedTime >= transitionTiming)
            {
                _characterMovement.characterControl.StartRunning();
            }
        }

        public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
        {
            
        }
    }
}