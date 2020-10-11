using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Undercooked
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Physics")]
        [SerializeField] private Rigidbody playerRigidbody;

        private InteractableController _interactableController;
        
        [Header("Animation")]
        [SerializeField] private Animator playerAnimator;
        private readonly int _playerMovementID = Animator.StringToHash("Movement");
        private readonly int _playerGrabID = Animator.StringToHash("Grab");

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;

        private const string ActionMapGameplay = "PlayerControls";
        private const string ActionMapMenu = "MenuControls";

        private Vector3 _inputDirection;

        //  Dashing
        [SerializeField] private float dashForce = 400f;
        private bool _isDashing = false;
        private bool _isDashingPossible = true;
        private readonly WaitForSeconds _dashDuration = new WaitForSeconds(0.17f);
        private readonly WaitForSeconds _dashCooldown = new WaitForSeconds(0.07f);
        
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 5f;
  
        private InputAction _moveAction;
        private InputAction _dashAction;

        private void Awake()
        {
            _interactableController = GetComponentInChildren<InteractableController>();
        }

        private void Start()
        {
            _moveAction = playerInput.currentActionMap["Move"];
            _dashAction = playerInput.currentActionMap["Dash"];
            
            EnableGameplayControls();
            playerInput.currentActionMap["Action"].performed += HandleAction;
            playerInput.currentActionMap["Move"].performed += HandleMove;
            _dashAction.performed += HandleDash;            
            playerInput.currentActionMap.Enable();
        }

        private void HandleDash(InputAction.CallbackContext context)
        {
            if (!_isDashingPossible) return;
            StartCoroutine(Dash());
        }

        private IEnumerator Dash()
        {
            _isDashingPossible = false;
            //Debug.Log("[PlayerController] Dash");
            playerRigidbody.AddRelativeForce(dashForce * Vector3.forward);
            yield return new WaitForFixedUpdate();
            _isDashing = true;
            yield return _dashDuration;
            // Debug.Log("[PlayerController] Dash finished");
            _isDashing = false;
            yield return _dashCooldown;
            // Debug.Log("[PlayerController] Dash Cooldown is over");
            _isDashingPossible = true;
        }
        
        private void HandleMove(InputAction.CallbackContext context)
        {
            // TODO: Processors on input binding not working for analogical stick. Investigate it.
            Vector2 inputMovement = context.ReadValue<Vector2>();
            if (inputMovement.x > 0.3f)
            {
                inputMovement.x = 1f;
            }
            else if (inputMovement.x < -0.3)
            {
                inputMovement.x = -1f;
            }
            else
            {
                inputMovement.x = 0f;
            }

            if (inputMovement.y > 0.3f)
            {
                inputMovement.y = 1f;
            }
            else if (inputMovement.y < -0.3f)
            {
                inputMovement.y = -1f;
            }
            else
            {
                inputMovement.y = 0f;
            }
            
            _inputDirection = new Vector3(inputMovement.x, 0, inputMovement.y);
        }

        private void HandleAction(InputAction.CallbackContext context)
        {
            Debug.Log("[PlayerController] Action");
            // currentInteractable could be null
            _interactableController.CurrentInteractable?.Interact();
        }

        private void Update()
        {
            CalculateInputDirection();
        }

        private void FixedUpdate()
        {
            
            MoveThePlayer();
            //AnimatePlayerMovement();
            TurnThePlayer();
        }
        
        private void MoveThePlayer()
        {
            if (_isDashing)
            {
                // keep the current velocity, only redirecting
                var currentVelocity = playerRigidbody.velocity.magnitude;
            
                // velocity could fall lower than movement speed, while dashing
                //currentVelocity = Mathf.Max(currentVelocity, movementSpeed);
                // if (currentVelocity < movementSpeed)
                // {
                //     Debug.Log($"Dashing slower than walking {currentVelocity}");
                // }
                // else
                // {
                //     Debug.Log($"Dashing FASTER than walking {currentVelocity}");
                // }

                var inputNormalized = _inputDirection.normalized;
                if (inputNormalized == Vector3.zero)
                {
                    inputNormalized = transform.forward;
                }
                playerRigidbody.velocity = inputNormalized * currentVelocity;
            }
            else
            {
                playerRigidbody.velocity = _inputDirection.normalized * movementSpeed;
            }
        }

        private void CalculateInputDirection()
        {
            var inputMovement = _moveAction.ReadValue<Vector2>();
            if (inputMovement.x > 0.3f)
            {
                inputMovement.x = 1f;
            }
            else if (inputMovement.x < -0.3)
            {
                inputMovement.x = -1f;
            }
            else
            {
                inputMovement.x = 0f;
            }

            if (inputMovement.y > 0.3f)
            {
                inputMovement.y = 1f;
            }
            else if (inputMovement.y < -0.3f)
            {
                inputMovement.y = -1f;
            }
            else
            {
                inputMovement.y = 0f;
            }

            _inputDirection = new Vector3(inputMovement.x, 0f, inputMovement.y);
        }

        private void TurnThePlayer()
        {
            if (!(playerRigidbody.velocity.magnitude > 0.1f) || _inputDirection == Vector3.zero) return;
            
            Quaternion newRotation = Quaternion.LookRotation(_inputDirection);
            transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.deltaTime * 15f);
        }

        private void AnimatePlayerMovement()
        {
            playerAnimator.SetFloat(_playerMovementID, _inputDirection.sqrMagnitude);
        }
        
        //Switching Action Maps ----

        public void EnableGameplayControls()
        {
            playerInput.SwitchCurrentActionMap(ActionMapGameplay);  
        }

        public void EnablePauseMenuControls()
        {
            playerInput.SwitchCurrentActionMap(ActionMapMenu);
        }
        
        public PlayerInput GetPlayerInput()
        {
            return playerInput;
        } 
    }
}
