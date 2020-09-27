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
    [SerializeField] private float movementSpeed = 3f;
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
        playerRigidbody.AddForce(dashForce * transform.forward, ForceMode.Impulse);
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
        //CalculateDesiredDirectionRelativeToCamera();
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

    private void ConvertDirectionFromRawToSmooth()
    {   
        if (_currentInput == true)
        {
            //_smoothDirection = Vector3.Lerp(_smoothDirection, _rawDirection, Time.deltaTime * smoothingSpeed);
        }
        else if (_currentInput == false)
        {
            _smoothDirection = Vector3.zero;
        }
        
    }

    
    private void MoveThePlayer()
    {
        if (playerRigidbody.velocity.sqrMagnitude > maximumVelocitySquared)
        {
            return;
        }
        
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
        playerRigidbody.AddForce(_inputDirection * _force);
        //Debug.Log(_inputDirection);
        // if (_currentInput == true)
        // {
        //     
        //     // _movement.Set(_smoothDirection.x, 0f, _smoothDirection.z);
        //     // _movement = _movement.normalized * movementSpeed * Time.deltaTime;
        //     // playerRigidbody.MovePosition(transform.position + _movement);
        // }
    }

    private void TurnThePlayer()
    {
        // TODO: tweak for Dash 
        if (playerRigidbody.velocity.magnitude > 0.1f && _inputDirection != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(_inputDirection);
            //transform.rotation.SetLookRotation(_inputDirection);
            transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.deltaTime * 10f);
            //transform.Rotate(Vector3.up * (angularVelocity * Time.deltaTime));
//            transform.LookAt(_inputDirection, Vector3.up);
        }

        
        //playerRigidbody.MoveRotation(newRotation);


        // if (_currentInput == true)
        // {
        //     Quaternion newRotation = Quaternion.LookRotation(_smoothDirection);
        //     playerRigidbody.MoveRotation(newRotation);
        // }
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
