using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Camera _mainCamera;

    [Header("Physics")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator;
    private readonly int _playerMovementID = Animator.StringToHash("Movement");
    private readonly int _playerAttackID = Animator.StringToHash("Attack");

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    private const string ActionMapGameplay = "PlayerControls";
    private const string ActionMapMenu = "MenuControls";

    private Vector3 _inputDirection;
    private Vector2 _movementInput;
    private bool _currentInput = false;

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float _force = 200f;
    [SerializeField] private float maximumVelocitySquared = 130f;
    [SerializeField] private float smoothingSpeed = 1f;
    [SerializeField] private float dashForce = 400f;
    [SerializeField] private float angularVelocity = 10f;
    
    private Vector3 _currentDirection;
    private Vector3 _rawDirection;
    private Vector3 _smoothDirection;
    private Vector3 _movement;

    private InputAction _moveAction;
    private InputAction _dashAction;

    private void Start()
    {
        _moveAction = playerInput.currentActionMap["Move"];
        _dashAction = playerInput.currentActionMap["Dash"];
        
        FindCamera();
        EnableGameplayControls();
        playerInput.currentActionMap["Action"].performed += HandleAction;
        //playerInput.currentActionMap["Move"].performed += HandleMove;
        _dashAction.performed += HandleDash;
        playerInput.currentActionMap.Enable();
    }

    private void HandleDash(InputAction.CallbackContext obj)
    {
        //playerRigidbody.AddForce(dashForce * transform.forward, ForceMode.Impulse);
        if (!_isDashingPossible) return;
        StartCoroutine(DashCooldown());
    }

    private bool _isDashing = false;
    private bool _isDashingPossible = true;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 0.1f;
    
    private IEnumerator DashCooldown()
    {
        _isDashingPossible = false;
        Debug.Log("Dash");
        //playerRigidbody.AddForce(dashForce * transform.forward);
        //playerRigidbody.velocity += dashSpeed * transform.forward;
        playerRigidbody.AddRelativeForce(dashForce * Vector3.forward);
        yield return new WaitForFixedUpdate();
        _isDashing = true;
        
        yield return new WaitForSeconds(_dashDuration);
        Debug.Log("Dash finished");
        _isDashing = false;
        yield return new WaitForSeconds(_dashCooldown);
        Debug.Log("Dash Cooldown UP");
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
        
        Debug.Log($"Move {inputMovement}");
        _inputDirection = new Vector3(inputMovement.x, 0, inputMovement.y);
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        Debug.Log($"Action");
    }

    private void FindCamera()
    {
        //_mainCamera = GameManager.Instance.mainCamera;
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        //CalculateMovementInput();
    }

    private void FixedUpdate()
    {
        CalculateInputDirection();
        //ConvertDirectionFromRawToSmooth();
        MoveThePlayer();
        //AnimatePlayerMovement();
        TurnThePlayer();
    }

    private void CalculateMovementInput()
    {
        if (_inputDirection == Vector3.zero)
        {
            _currentInput = false;
        }
        else if (_inputDirection != Vector3.zero)
        {
            _currentInput = true;
        }
    }
    
    private void CalculateDesiredDirectionRelativeToCamera()
    {
        //Camera Direction
	    var cameraForward = _mainCamera.transform.forward;
	    var cameraRight = _mainCamera.transform.right;

	    cameraForward.y = 0f;
	    cameraRight.y = 0f;

        _rawDirection = cameraForward * _inputDirection.z + cameraRight * _inputDirection.x;
    }

    private void MoveThePlayer()
    {
        if (_isDashing)
        {
            // keep the current velocity, only redirecting
            var currentVelocity = playerRigidbody.velocity.magnitude;
            
            // velocity could fall lower than movement speed, while dashing
            //currentVelocity = Mathf.Max(currentVelocity, movementSpeed);
            if (currentVelocity < movementSpeed)
            {
//                Debug.Log($"Dashing slower than walking {currentVelocity}");
            }
            else
            {
//                Debug.Log($"Dashing FASTER than walking {currentVelocity}");
            }

            var inputNormalized = _inputDirection.normalized;
            if (inputNormalized == Vector3.zero)
            {
                //Debug.Log("Input Normalized is zero");
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
        if (playerRigidbody.velocity.magnitude > 0.1f && _inputDirection != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(_inputDirection);
            transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.deltaTime * 15f);
        }
    }

    private void AnimatePlayerMovement()
    {
        playerAnimator.SetFloat(_playerMovementID, _inputDirection.sqrMagnitude);
    }
    

    private void OnOpenPauseMenu(InputValue value)
    {
        // if(value.isPressed)
        // {
        //     GameManager.Instance.TogglePauseMenu(true);
        // }
    }

    private void OnClosePauseMenu(InputValue value)
    {
        // if(value.isPressed)
        // {
        //     GameManager.Instance.TogglePauseMenu(false);
        // }
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
