using UnityEngine;

namespace Dev.Scripts.Characters
{
    public class CharacterInputController : MonoBehaviour
    {
        // Singleton instance
        private static CharacterInputController _instance;
        public static CharacterInputController Instance => _instance;

        #region Public Variables
        public bool swipeLeft, swipeRight, swipeUp, swipeDown;
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
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
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
                    swipeDown = true;
                }
                else
                {
                    swipeUp = true;
                }
            }
            else
            {
                if (swipeDelta.x < 0)
                {
                    swipeLeft = true;
                }
                else
                {
                    swipeRight = true;
                }
            }
            Reset();
        }

        private void ResetSwipeFlags()
        {
            swipeDown = swipeUp = swipeLeft = swipeRight = false;
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