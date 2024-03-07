using Dev.Scripts.Camera;
using Dev.Scripts.Characters;
using Dev.Scripts.Track;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

namespace Characters
{
    public class CharacterMovement : MonoBehaviour
    {
        #region Public Variables
        
        [Space,Header("Helpers")]
        public TrackManager trackManager;
        public CharacterController controller;
        
        [Space,Header("Movement Settings")]
        public float laneChangeSpeed = 2f;

        [Space,Header("Jump Settings")]
        public Transform groundCheck;
        public float gravity = -9.81f;
        public float jumpHeight = 3.0f;
        
        [Space,Header("Camera Settings")]
        public CameraController cameraController;
        public CameraShake cameraShake;

        #endregion

        #region Getter Setter
        public int DesiredLane
        {
            get => _desiredLane;
            set => _desiredLane = value;
        }
        
        #endregion
        
        #region Debugging Variables

        [Space,Header("Debug")]
        [SerializeField,ReadOnly]
        private Vector3 movementDirection;

        [ReadOnly]
        public Vector3 velocity;

        [ReadOnly]
        public bool isGrounded;

        #endregion

        #region Private Variables
        
        private int _layerMask = (1 << 6) | (1 << 9);
        private int _desiredLane = 1;

        #endregion

        #region Private Methods
        
        private void Awake()
        {
            SignUpEvents();
        }
        private void SignUpEvents()
        {
            GameEvents.RunStartEvent += () => PlayAnim(TransitionParameter.Running.ToString(), 0.1f, 1f);
            GameEvents.PlayerDeathEvent += () => PlayAnim(TransitionParameter.Dead.ToString(), 0.01f, 1f);
        }
        private IEnumerator CharacterRotation(Quaternion rot)
        {
            float dur = 0;
            var characterControl = trackManager.characterController;

            while (dur < .2f)
            {
                characterControl.character.transform.rotation = Quaternion.Slerp(characterControl.character.transform.rotation,rot,.2f);
                dur += Time.deltaTime;
                yield return null;
            }

            while (dur < .4f)
            {
                characterControl.character.transform.rotation = Quaternion.Slerp(characterControl.character.transform.rotation,Quaternion.identity, .3f);
                dur += Time.deltaTime;
                yield return null;
            }
            characterControl.character.transform.rotation = Quaternion.identity;
        }
        
        #endregion
        
        #region Public State Methods

        public void Move()
        {
            movementDirection.z = trackManager.characterController.trackManager.Speed;
            var position = transform.position;
            var targetPosition = position.z * Vector3.forward + position.y * Vector3.up;

            if (CharacterInputController.SwipeRight && _desiredLane < 2)
            {
                StartCoroutine(CharacterRotation(Quaternion.Euler(0, 90, 0)));
                _desiredLane++;
            }
            else if (CharacterInputController.SwipeLeft && _desiredLane > 0)
            {
                StartCoroutine(CharacterRotation(Quaternion.Euler(0, -90, 0)));
                _desiredLane--;
            }

            if (_desiredLane == 0)
            {
                targetPosition += Vector3.left * trackManager.laneOffset;
            }
            else if (_desiredLane == 2)
            {
                targetPosition += Vector3.right * trackManager.laneOffset;
            }
            
            if (!transform.position.Equals(targetPosition))
            {
                Vector3 diff = targetPosition - transform.position;
            
                Vector3 moveDir = diff * (laneChangeSpeed * Time.deltaTime);
            
                controller.Move(diff* (laneChangeSpeed * Time.deltaTime));

                if (moveDir.sqrMagnitude < diff.magnitude)
                {
                    controller.Move(moveDir);
                }
                else
                {
                    controller.Move(diff);
                }
            }
            controller.Move(movementDirection * Time.deltaTime);
        }
        public void GroundCheck()
        {
            bool grounded = Physics.CheckSphere(groundCheck.position, 0.03f, _layerMask);

            isGrounded = grounded;
            
            if (grounded && velocity.y <0)
                velocity.y = 0f;
        
            if(!grounded)
                velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }
        public void Jump()
        {
            controller.height =1f;
            controller.center = new Vector3(0,1.5f,0);
            velocity.y = Mathf.Sqrt(jumpHeight * -1.5f * gravity);
        }
        public void PlayAnim(string animationName,float animTransSpeed,float animationSpeed)
        {
            var characterControl = trackManager.characterController;
            
            if(characterControl.character.animator.IsInTransition(0)) 
                return;
            characterControl.character.animator.CrossFade(animationName, animTransSpeed);
            characterControl.character.animator.speed = animationSpeed;
        }

        #endregion
        
    }
}