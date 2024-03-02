using UnityEngine;

namespace Dev.Scripts.Character.CharacterStates
{
    public abstract class StateData: ScriptableObject
    {
        public abstract void OnEnter(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo);
        public abstract void UpdateAbility(BaseState characterState, Animator animator,AnimatorStateInfo  stateInfo);
        public abstract void OnExit(BaseState characterState, Animator animator, AnimatorStateInfo stateInfo);
    }
}