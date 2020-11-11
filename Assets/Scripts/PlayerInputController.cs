using Undercooked;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private PlayerController playerController1;
    [SerializeField] private PlayerController playerController2;

    private PlayerController _activePlayerController;
    private bool isFirstPlayerControllerActive;

    private InputAction _switchAvatarAction;
    
    public delegate void SwitchPlayerController(PlayerControllerIndex playerControllerIndex);
    public static SwitchPlayerController OnSwitchPlayerController;

    private void EnableFirstPlayerController()
    {
        _activePlayerController = playerController1;
        playerController2.DisableController();
        playerController1.EnableController();
        isFirstPlayerControllerActive = true;
        OnSwitchPlayerController(PlayerControllerIndex.First);
    }

    private void TogglePlayerController()
    {
        if (isFirstPlayerControllerActive)
        {
            _activePlayerController = playerController2;
            playerController1.DisableController();
            //maybe wait a bit? play effects?
            playerController2.EnableController();
            OnSwitchPlayerController(PlayerControllerIndex.Second);
        }
        else
        {
            _activePlayerController = playerController1;
            playerController2.DisableController();
            playerController1.EnableController();
            OnSwitchPlayerController(PlayerControllerIndex.First);
        }
        isFirstPlayerControllerActive = !isFirstPlayerControllerActive;

    }

    private void Awake()
    {
        _switchAvatarAction = playerInput.currentActionMap["SwitchAvatar"];
        var map = playerInput.currentActionMap;
    }

    private void Start()
    {
        EnableFirstPlayerController();
    }

    private void OnEnable()
    {
        SubscribeInputEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeInputEvents();
    }

    private void SubscribeInputEvents()
    {
        playerInput.onDeviceLost += HandleDeviceLost;
        playerInput.onDeviceRegained += HandleDeviceRegained;
        playerInput.onActionTriggered += HandleActionTriggered;
        playerInput.onControlsChanged += HandleControlsChanged;

        _switchAvatarAction.performed += HandleSwitchAvatar;
    }

    private void UnsubscribeInputEvents()
    {
        playerInput.onDeviceLost -= HandleDeviceLost;
        playerInput.onDeviceRegained -= HandleDeviceRegained;
        playerInput.onActionTriggered -= HandleActionTriggered;
        playerInput.onControlsChanged -= HandleControlsChanged;
        
        _switchAvatarAction.performed -= HandleSwitchAvatar;
    }
    
    private void HandleSwitchAvatar(InputAction.CallbackContext obj)
    {
        TogglePlayerController();
    }

    private void HandleControlsChanged(PlayerInput obj)
    {
        Debug.Log($"ControlsChanged {playerInput.currentControlScheme}");
        //TODO: Controls change dynamically
    }

    private void HandleActionTriggered(InputAction.CallbackContext context)
    {
        Debug.Log($"ActionTriggered {context.action.name} phase: {context.action.phase}");
    }
    
    private void HandleDeviceLost(PlayerInput context)
    {
        Debug.Log("Device Lost");
        //TODO: pause game with a warning
    }
    
    private void HandleDeviceRegained(PlayerInput context)
    {
        Debug.Log("Device Regained");
        //TODO: notify player and keep it paused (we could resume after some countdown)
    }

    public void Action(InputAction.CallbackContext context)
    {
        Debug.Log("Action");
    }
    
}
