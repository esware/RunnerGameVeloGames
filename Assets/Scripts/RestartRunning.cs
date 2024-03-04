using Dev.Scripts.Track;
using UnityEngine;

public class RestartRunning : StateMachineBehaviour
{
	static int s_DeadHash = Animator.StringToHash("Dead");

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (animator.GetBool(s_DeadHash))
            return; 
        
        TrackManager.Instance.StartMove();
    }

}
