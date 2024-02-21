using Dev.Scripts.Track;
using UnityEngine;

namespace Dev.Scripts.Characters
{
    public class CharacterInputController : MonoBehaviour
	{

	public static bool  SwipeLeft, SwipeRight, SwipeUp, SwipeDown;
	public bool tutorialHitObstacle {  get { return _tutorialHitObstacle;} set { _tutorialHitObstacle = value;} }
	public bool GetInputs
	{
		get => _getInputs;
		set => _getInputs = value;
	}
	
	[HideInInspector] public int currentTutorialLevel;
	[HideInInspector] public bool tutorialWaitingForValidation;
	
	private Vector2 _startingTouch;
	private bool _tutorialHitObstacle;
	private bool _isSwiping = false;
	private bool _getInputs = false;

	private void Awake()
	{
		_getInputs = false;
	}

	private bool TutorialMoveCheck(int tutorialLevel)
	{
		
		tutorialWaitingForValidation = currentTutorialLevel != tutorialLevel;

		return (!TrackManager.Instance.isTutorial || currentTutorialLevel >= tutorialLevel);
	}
    private void GetMobileInputs()
    {
	    if(!_getInputs)
		    return;
	    
	    SwipeDown = SwipeUp = SwipeLeft = SwipeRight = false;
	    
	    if(Input.GetMouseButtonDown(0))
	    {
		    _startingTouch = Input.mousePosition;
		    _isSwiping = true;
	    }
	    else if(Input.GetMouseButtonUp(0))
	    {
		    Reset();
	    }
	    if (Input.GetMouseButton(0))
	    {
		    if(_isSwiping)
		    {
			    var diff = (Vector2)Input.mousePosition - _startingTouch;

			    // Put difference in Screen ratio, but using only width, so the ratio is the same on both
			    // axes (otherwise we would have to swipe more vertically...)
			    diff = new Vector2(diff.x/Screen.width, diff.y/Screen.width);

			    if(diff.magnitude > .01f) //we set the swip distance to trigger movement to 1% of the screen width
			    {
				    if(Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
				    {
					    if(TutorialMoveCheck(2) && diff.y < 0)
					    {
						    SwipeDown = true;
					    }
					    else if(TutorialMoveCheck(1))
					    {
						    SwipeUp = true;
					    }
				    }
				    else if(TutorialMoveCheck(0))
				    {
					    if(diff.x < 0)
					    {
						    SwipeLeft = true;
					    }
					    else
					    {
						    SwipeRight = true;
					    }
				    }
				    Reset();
			    }
		    }
	    }
    }
    private void Reset()
    {
	    _startingTouch  = Vector2.zero;
	    _isSwiping = false;
    }
    private void Update ()
    {
	    // Use touch input on mobile
		GetMobileInputs();
    }
    
}
}