using Dev.Scripts.Camera;
using Dev.Scripts.Characters;
using Dev.Scripts.Track;


namespace Characters
{
    using System.Collections;
    using UnityEngine;

    public class CharacterMovement : MonoBehaviour
    {
        
        public TrackManager trackManager;
        public CharacterControl characterControl;
        
        [Space,Header("Movement Settings")]
        public Vector3 velocity;
        public float laneChangeSpeed = 2f;

        [Space,Header("Jump Settings")]
        public float gravity = -9.81f;
        public float jumpHeight = 3.0f;
        public bool isGrounded;
        
        public CameraController cameraController;
        public CameraShake cameraShake;

        public bool isSliding;
        public bool isJumping;
        
        public int DesiredLane
        {
            get => _desiredLane;
            set => _desiredLane = value;
        }
        private int _desiredLane = 1;//0:left, 1:middle, 2:right
        public Vector3 movementDirection;

        public CharacterController controller;

        private void Start()
        {
            SignUpEvents();
        }
        private void SignUpEvents()
        {
            GameEvents.RunStartEvent += () => PlayAnim(TransitionParameter.Running.ToString(), 0.1f, 1f);
            GameEvents.PlayerDeathEvent += () => PlayAnim(TransitionParameter.Dead.ToString(), 0.01f, 1f);
            //GameEvents.GameStartEvent += () => ChangeState(new CharacterIdleState(this));
        }
        public void Move()
        {
            movementDirection.z = trackManager.speed;
            GroundCheck();
            var targetPosition = transform.position.z * Vector3.forward + transform.position.y * Vector3.up;

            if (CharacterInputController.SwipeRight)
            {
                StartCoroutine(CharacterRotation(Quaternion.Euler(0, 90, 0)));
                _desiredLane++;
                if (_desiredLane == 3)
                    _desiredLane = 2;
            }
            if (CharacterInputController.SwipeLeft)
            {
                StartCoroutine(CharacterRotation(Quaternion.Euler(0, -90, 0)));
                _desiredLane--;
                if (_desiredLane == -1)
                    _desiredLane = 0;
            }
        
            if (_desiredLane == 0)
            {
                targetPosition += Vector3.left * trackManager.laneOffset;
            }
            else if (_desiredLane== 2)
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
            controller.Move(movementDirection* Time.deltaTime);

        }
        private IEnumerator CharacterRotation(Quaternion rot)
        {
            float dur = 0;
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
        public void GroundCheck()
        {
            bool grounded = Physics.CheckSphere(transform.position, .5f, LayerMask.GetMask("Ground"));

            isGrounded = grounded;
            
            if (grounded && velocity.y <0)
                velocity.y = -1f;
        
            if(!grounded)
                velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }
        public void Jump()
        {
            isJumping = true;
            controller.height =1f;
            controller.center = new Vector3(0,1.5f,0);
            velocity.y = Mathf.Sqrt(jumpHeight * -1.5f * gravity);
        }
        public void PlayAnim(string animation,float animTransSpeed,float animationSpeed)
        {
            if(characterControl.character.animator.IsInTransition(0)) {return;}
            characterControl.character.animator.CrossFade(animation, animTransSpeed);
            characterControl.character.animator.speed = animationSpeed;
        }

    }
}