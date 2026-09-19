using System.Collections;
using Lean.Transition;
using Undercooked.Appliances;
using Undercooked.Model;
using Undercooked.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Undercooked.Player
{
    public class PlayerController : MonoBehaviour
    {
        private const string IsCleaningParameter = "isCleaning";
        private const string HasPickupParameter = "hasPickup";
        private const string IsChoppingParameter = "isChopping";
        private const string VelocityParameter = "velocity";
        private const string MoveAction = "Move";
        private const string DashAction = "Dash";
        private const string PickUpAction = "PickUp";
        private const string InteractAction = "Interact";
        private const string StartAtPlayerAction = "Start@Player";

        [SerializeField] private Color playerColor;
        [SerializeField] private Transform selector;
        [SerializeField] private Material playerUniqueColorMaterial;

        [Header("Physics")]
        [SerializeField] private Rigidbody playerRigidbody;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        private readonly int _isCleaningHash = Animator.StringToHash(IsCleaningParameter);
        private readonly int _hasPickupHash = Animator.StringToHash(HasPickupParameter);
        private readonly int _isChoppingHash = Animator.StringToHash(IsChoppingParameter);
        private readonly int _velocityHash = Animator.StringToHash(VelocityParameter);

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        private InputAction _moveAction;
        private InputAction _dashAction;
        private InputAction _pickUpAction;
        private InputAction _interactAction;
        private InputAction _startAtPlayerAction;

        // Dashing
        [SerializeField] private float dashForce = 900f;
        private bool _isDashing = false;
        private bool _isDashingPossible = true;
        private readonly WaitForSeconds _dashDuration = new(0.17f);
        private readonly WaitForSeconds _dashCooldown = new(0.07f);

        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 5f;

        private InteractableController _interactableController;
        private bool _isActive;
        private IPickable _currentPickable;
        private Vector3 _inputDirection;
        private bool _hasSubscribedControllerEvents;

        [SerializeField] private Transform slot;
        [SerializeField] private ParticleSystem dashParticle;
        [SerializeField] private Transform knife;

        [Header("Audio")]
        [SerializeField] private AudioClip dashAudio;
        [SerializeField] private AudioClip pickupAudio;
        [SerializeField] private AudioClip dropAudio;

        private void Awake()
        {
            _moveAction = playerInput.currentActionMap[MoveAction];
            _dashAction = playerInput.currentActionMap[DashAction];
            _pickUpAction = playerInput.currentActionMap[PickUpAction];
            _interactAction = playerInput.currentActionMap[InteractAction];
            _startAtPlayerAction = playerInput.currentActionMap[StartAtPlayerAction];

            _interactableController = GetComponentInChildren<InteractableController>();
            knife.gameObject.SetActive(false);

            SetPlayerUniqueColor(playerColor);
        }

        private void SetPlayerUniqueColor(Color color)
        {
            selector.GetComponent<MeshRenderer>().material.color = color;
            playerUniqueColorMaterial.color = color;
        }

        public void ActivatePlayer()
        {
            _isActive = true;
            SubscribeControllerEvents();
            selector.gameObject.SetActive(true);
        }

        public void DeactivatePlayer()
        {
            _isActive = false;
            UnsubscribeControllerEvents();
            animator.SetFloat(_velocityHash, 0f);
            selector.gameObject.SetActive(false);
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
                    this.PlaySoundTransition(pickupAudio);
                    _currentPickable.Pick();
                    _interactableController.Remove(_currentPickable as Interactable);
                    _currentPickable.gameObject.transform.SetPositionAndRotation(slot.transform.position,
                        Quaternion.identity);
                    _currentPickable.gameObject.transform.SetParent(slot);
                    return;
                }

                // Interactable only (not a IPickable)
                _currentPickable = interactable?.TryToPickUpFromSlot(_currentPickable);
                if (_currentPickable != null)
                {
                    animator.SetBool(_hasPickupHash, true);
                    this.PlaySoundTransition(pickupAudio);
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
                this.PlaySoundTransition(dropAudio);
                _currentPickable.Drop();
                _currentPickable = null;
                return;
            }

            // we carry a pickable and we have an interactable in range
            // we may drop into the interactable

            // Try to drop on the interactable. It may refuse it, e.g. dropping a plate into the CuttingBoard,
            // or simply it already have something on it

            bool dropSuccess = interactable.TryToDropIntoSlot(_currentPickable);
            if (!dropSuccess) return;

            animator.SetBool(_hasPickupHash, false);
            this.PlaySoundTransition(dropAudio);
            _currentPickable = null;
        }

        private void HandleMove(InputAction.CallbackContext context)
        {
            // TODO: Processors on input binding not working for analogical stick. Investigate it.
            _inputDirection = GetInputDirection(context.ReadValue<Vector2>());
        }

        private void HandleInteract(InputAction.CallbackContext context)
        {
            _interactableController.CurrentInteractable?.Interact(this);
        }

        private void HandleStart(InputAction.CallbackContext context)
        {
            MenuPanelUI.PauseUnpause();
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
                var currentVelocity = playerRigidbody.linearVelocity.magnitude;

                var inputNormalized = _inputDirection.normalized;
                if (inputNormalized == Vector3.zero)
                {
                    inputNormalized = transform.forward;
                }

                playerRigidbody.linearVelocity = inputNormalized * currentVelocity;
            }
            else
            {
                playerRigidbody.linearVelocity = _inputDirection.normalized * movementSpeed;
            }
        }

        private void CalculateInputDirection()
        {
            _inputDirection = GetInputDirection(_moveAction.ReadValue<Vector2>());
        }

        private static Vector3 GetInputDirection(Vector2 inputMovement)
        {
            var horizontal = Mathf.Abs(inputMovement.x) > 0.3f ? Mathf.Sign(inputMovement.x) : 0f;
            var vertical = Mathf.Abs(inputMovement.y) > 0.3f ? Mathf.Sign(inputMovement.y) : 0f;
            return new Vector3(horizontal, 0f, vertical);
        }

        private void TurnThePlayer()
        {
            if ((playerRigidbody.linearVelocity.magnitude <= 0.1f) || _inputDirection == Vector3.zero) return;

            Quaternion newRotation = Quaternion.LookRotation(_inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 15f);
        }

        private void AnimatePlayerMovement()
        {
            animator.SetFloat(_velocityHash, _inputDirection.sqrMagnitude);
        }
    }
}
