using Dev.Scripts.Camera;
using Dev.Scripts.Characters;
using Dev.Scripts.Track;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using Unity.Mathematics;
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
        private void CharacterRotation(Quaternion rot)
        {
            var character = trackManager.characterController.character;

            character.transform.DORotateQuaternion(rot, 0.2f).OnComplete(() =>
            {
                character.transform.DORotateQuaternion(Quaternion.identity, 0.2f);
            });
        }
        
        #endregion
        
        #region Public State Methods

        public void Move()
        {
            movementDirection.z = trackManager.characterController.trackManager.Speed;
            var position = transform.position;
            var targetPosition = position.z * Vector3.forward + position.y * Vector3.up;

            if (CharacterInputController.Instance.swipeRight && _desiredLane < 2)
            {
                CharacterRotation(Quaternion.Euler(0, 70, 0));
                _desiredLane++;
            }
            else if (CharacterInputController.Instance.swipeLeft && _desiredLane > 0)
            {
                CharacterRotation(Quaternion.Euler(0, -70, 0));
                _desiredLane--;
            }

            switch (_desiredLane)
            {
                case 0:
                    targetPosition += Vector3.left * trackManager.laneOffset;
                    break;
                case 2:
                    targetPosition += Vector3.right * trackManager.laneOffset;
                    break;
            }
            
            if (!transform.position.Equals(targetPosition))
            {
                var diff = targetPosition - transform.position;
            
                var moveDir = diff * (laneChangeSpeed * Time.deltaTime);
            
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
            ApplyGravity();
        }
        private void ApplyGravity()
        {
            int mask = 1 << 6 | 1 << 9;
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f,mask);

            if (isGrounded && velocity.y < 0.0f)
                velocity.y = 0f;

            if (!isGrounded)
                velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }
        public float DistanceToGround()
        {
            if (Physics.Raycast(groundCheck.position, Vector3.down, out var hit, Mathf.Infinity, _layerMask))
            {
                return hit.distance;
            }
            return -1f;
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