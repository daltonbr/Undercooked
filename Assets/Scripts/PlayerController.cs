using System.Collections;
using Lean.Transition;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Undercooked
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Physics")] [SerializeField] private Rigidbody playerRigidbody;

        [Header("Animation")] [SerializeField] private Animator animator;
        private readonly int _isCleaningHash = Animator.StringToHash("isCleaning");
        private readonly int _hasPickupHash = Animator.StringToHash("hasPickup");
        private readonly int _isChoppingHash = Animator.StringToHash("isChopping");
        private readonly int _velocityHash = Animator.StringToHash("velocity");
        
        [Header("Input")] [SerializeField] private PlayerInput playerInput;

        private const string ActionMapGameplay = "PlayerControls";
        private const string ActionMapMenu = "MenuControls";
        
        // Dashing
        [SerializeField] private float dashForce = 400f;
        private bool _isDashing = false;
        private bool _isDashingPossible = true;
        private readonly WaitForSeconds _dashDuration = new WaitForSeconds(0.17f);
        private readonly WaitForSeconds _dashCooldown = new WaitForSeconds(0.07f);

        [Header("Movement Settings")] [SerializeField]
        private float movementSpeed = 5f;
        
        private InteractableController _interactableController;
        private bool _isActive;
        private IPickable _currentPickable;
        private Vector3 _inputDirection;
        private bool _hasSubscribedControllerEvents;
        
        [SerializeField] private Transform slot;
        
        private InputAction _moveAction;
        private InputAction _dashAction;
        private InputAction _pickUpAction;
        private InputAction _interactAction;

        [SerializeField] private ParticleSystem dashParticle;
        [SerializeField] private AudioClip dashAudio;
        [SerializeField] private Transform knife;

        private void Awake()
        {
            _interactableController = GetComponentInChildren<InteractableController>();
            knife.gameObject.SetActive(false);
        }
        
        public void EnableController()
        {
            _moveAction = playerInput.currentActionMap["Move"];
            _dashAction = playerInput.currentActionMap["Dash"];
            _pickUpAction = playerInput.currentActionMap["PickUp"];
            _interactAction = playerInput.currentActionMap["Interact"];

            EnableGameplayControls();
            playerInput.currentActionMap.Enable();

            SubscribeControllerEvents();
            _isActive = true;
        }
        
        public void DisableController()
        {
            _isActive = false;
            playerInput.currentActionMap.Disable();
            UnsubscribeControllerEvents();
            animator.SetFloat(_velocityHash, 0f);
        }

        private void OnEnable()
        {
            SubscribeInteractableEvents();
        }

        private void OnDisable()
        {
            UnsubscribeInteractableEvents();
        }

        private void SubscribeControllerEvents()
        {
            if (_hasSubscribedControllerEvents) return;
            _hasSubscribedControllerEvents = true;
            _moveAction.performed += HandleMove;
            _dashAction.performed += HandleDash;
            _pickUpAction.performed += HandlePickUp;
            _interactAction.performed += HandleInteract;
        }
        
        private void UnsubscribeControllerEvents()
        {
            if (_hasSubscribedControllerEvents == false) return;
            
            _hasSubscribedControllerEvents = false;
            _moveAction.performed -= HandleMove;
            _dashAction.performed -= HandleDash;
            _pickUpAction.performed -= HandlePickUp;
            _interactAction.performed -= HandleInteract;
        }

        private void SubscribeInteractableEvents()
        {
            ChoppingBoard.OnChoppingStart += HandleChoppingStart;
            ChoppingBoard.OnChoppingStop += HandleChoppingStop;
            Sink.OnCleanStart += HandleCleanStart;
            Sink.OnCleanStop += HandleCleanStop;
        }
        
        private void UnsubscribeInteractableEvents()
        {
            ChoppingBoard.OnChoppingStart -= HandleChoppingStart;
            ChoppingBoard.OnChoppingStop -= HandleChoppingStop;
            Sink.OnCleanStart -= HandleCleanStart;
            Sink.OnCleanStop -= HandleCleanStop;
        }

        private void HandleCleanStart(PlayerController playerController)
        {
            if (Equals(playerController) == false) return;
            
            animator.SetBool(_isCleaningHash, true);
        }

        private void HandleCleanStop(PlayerController playerController)
        {
            if (Equals(playerController) == false) return;
            
            animator.SetBool(_isCleaningHash, false);
        }

        private void HandleChoppingStart(PlayerController playerController)
        {
            if (Equals(playerController) == false) return;
            
            animator.SetBool(_isChoppingHash, true);
            knife.gameObject.SetActive(true);
        }

        private void HandleChoppingStop(PlayerController playerController)
        {
            if (Equals(playerController) == false) return;
            
            animator.SetBool(_isChoppingHash, false);
            knife.gameObject.SetActive(false);
        }

        private void HandleDash(InputAction.CallbackContext context)
        {
            if (!_isDashingPossible) return;
            StartCoroutine(Dash());
        }

        private IEnumerator Dash()
        {
            _isDashingPossible = false;
            playerRigidbody.AddRelativeForce(dashForce * Vector3.forward);
            dashParticle.Play();
            dashParticle.PlaySoundTransition(dashAudio);
            
            yield return new WaitForFixedUpdate();
            _isDashing = true;
            yield return _dashDuration;
            _isDashing = false;
            yield return _dashCooldown;
            _isDashingPossible = true;
        }

        private void HandlePickUp(InputAction.CallbackContext context)
        {
            var interactable = _interactableController.CurrentInteractable;
            
            // empty hands, try to pick
            if (_currentPickable == null)
            {
                _currentPickable = interactable as IPickable;
                if (_currentPickable != null)
                {
                    animator.SetBool(_hasPickupHash, true);
                    _currentPickable.Pick();
                    _interactableController.Remove(_currentPickable as Interactable);
                    _currentPickable.gameObject.transform.SetPositionAndRotation(slot.transform.position, Quaternion.identity);
                    _currentPickable.gameObject.transform.SetParent(slot);
                    return;
                }

                // Interactable only (not a IPickable)
                _currentPickable = interactable?.TryToPickUpFromSlot(_currentPickable);
                if (_currentPickable != null)
                {
                    animator.SetBool(_hasPickupHash, true);
                }
                _currentPickable?.gameObject.transform.SetPositionAndRotation(
                    slot.position, Quaternion.identity);
                _currentPickable?.gameObject.transform.SetParent(slot);
                return;
            }
            
            // we carry a pickable, let's try to drop it (we may fail)
            
            // no interactable in range or at most a Pickable in range (we ignore it)
            if (interactable == null || interactable is IPickable)
            {
                animator.SetBool(_hasPickupHash, false);
                _currentPickable.Drop();
                _currentPickable = null;
                return;
            }
            
            // we carry a pickable and we have an interactable in range
            // we may drop into the interactable
            
            // Try to drop on the interactable. It may refuse it, e.g. dropping a plate into the CuttingBoard,
            // or simply it already have something on it
            //Debug.Log($"[PlayerController] {_currentPickable.gameObject.name} trying to drop into {interactable.gameObject.name} ");

            bool dropSuccess = interactable.TryToDropIntoSlot(_currentPickable);
            if (!dropSuccess) return;
            
            animator.SetBool(_hasPickupHash, false);
            _currentPickable = null;
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

        private void HandleInteract(InputAction.CallbackContext context)
        {
            _interactableController.CurrentInteractable?.Interact(this);
        }

        private void Update()
        {
            if (!_isActive) return;
            CalculateInputDirection();
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;
            MoveThePlayer();
            AnimatePlayerMovement();
            TurnThePlayer();
        }
        
        private void MoveThePlayer()
        {
            if (_isDashing)
            {
                var currentVelocity = playerRigidbody.velocity.magnitude;

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
            animator.SetFloat(_velocityHash, _inputDirection.sqrMagnitude);
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
