using UnityEngine;

namespace Dev.Scripts.Characters
{
    public class CharacterInputController : MonoBehaviour
    {
        #region Public Variables

        public static bool SwipeLeft, SwipeRight, SwipeUp, SwipeDown;
        public bool GetInputs
        {
            get => _getInputs;
            set => _getInputs = value;
        }

        #endregion

        #region Private Variables

        private const float SwipeThreshold = 0.1f;
        
        private Vector2 _startingTouch;
        private bool _isSwiping = false;
        private bool _getInputs = false;

        #endregion

        #region Methods

        private void Awake()
        {
            _getInputs = false;
        }

        private void GetMobileInputs()
        {
            if (!_getInputs)
                return;

            ResetSwipeFlags();

            if (Input.GetMouseButtonDown(0))
            {
                _startingTouch = Input.mousePosition;
                _isSwiping = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Reset();
            }

            if (!Input.GetMouseButton(0)) return;

            if (!_isSwiping) return;
            
            var swipeDelta = (Vector2)Input.mousePosition - _startingTouch;

            swipeDelta = new Vector2(swipeDelta.x / Screen.width, swipeDelta.y / Screen.width);

            if (swipeDelta.magnitude < SwipeThreshold)
                return;

            if (Mathf.Abs(swipeDelta.y) > Mathf.Abs(swipeDelta.x))
            {
                if (swipeDelta.y < 0)
                {
                    SwipeDown = true;
                }
                else
                {
                    SwipeUp = true;
                }
            }
            else
            {
                if (swipeDelta.x < 0)
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

        private void ResetSwipeFlags()
        {
            SwipeDown = SwipeUp = SwipeLeft = SwipeRight = false;
        }

        private void Reset()
        {
            _startingTouch = Vector2.zero;
            _isSwiping = false;
        }

        private void Update()
        {
            GetMobileInputs();
        }

        #endregion
        
    }
}