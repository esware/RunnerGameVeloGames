using Characters;
using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    [CreateAssetMenu(menuName = "EWGames/CharacterStates/SlideState",fileName = "Sliding")]
    public class CharacterSlidingState : StateData
{
    [Range(0, 1f)]
    [SerializeField] private float transitionTiming;

    private CharacterMovement _characterMovement;

    public override void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
    {
        _characterMovement = characterState.GetCharacterMovement(animator);

        if (_characterMovement == null)
        {
            Debug.LogError("CharacterMovement component could not be found!");
            return;
        }

        _characterMovement.trackManager.Speed *= 1.3f;
        _characterMovement.gravity *= 3f;
        _characterMovement.controller.center = new Vector3(0, 0.5f, 0);
        _characterMovement.controller.height = 1f;
    }

    public override void UpdateAbility(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
    {
        if (_characterMovement == null)
        {
            Debug.LogError("CharacterMovement component is null during update!");
            return;
        }
        
        if (stateInfo.normalizedTime >= transitionTiming)
        {
            animator.SetBool(TransitionParameter.ForceTransition.ToString(), true);
            animator.SetBool(TransitionParameter.SlideEnd.ToString(), true);
        }
        
        _characterMovement.Move();
    }

    public override void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo)
    {
        if (_characterMovement == null)
        {
            Debug.LogError("CharacterMovement component is null during exit!");
            return;
        }
        _characterMovement.trackManager.Speed /= 1.3f;
        _characterMovement.gravity /= 3f;
        _characterMovement.controller.center = new Vector3(0, 1, 0);
        _characterMovement.controller.height = 2f;
        
        animator.SetBool(TransitionParameter.ForceTransition.ToString(), false);
        animator.SetBool(TransitionParameter.SlideEnd.ToString(), false);
    }
}

}