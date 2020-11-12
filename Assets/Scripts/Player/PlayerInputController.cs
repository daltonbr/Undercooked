using UnityEngine;
using UnityEngine.InputSystem;

namespace Undercooked.Player
{
    public class PlayerInputController : MonoBehaviour
    {
        public enum PlayerControllerIndex
        {
            None = 0,
            First = 1,
            Second = 2,
            Third = 3,
            Fourth = 4
        }
    
        [SerializeField] private PlayerInput playerInput;

        [SerializeField] private PlayerController playerController1;
        [SerializeField] private PlayerController playerController2;

        private bool isFirstPlayerControllerActive;

        private InputAction _switchAvatarAction;
    
        public delegate void SwitchPlayerController(PlayerControllerIndex playerControllerIndex);
        public static SwitchPlayerController OnSwitchPlayerController;

        private const string ActionMapGameplay = "PlayerControls";
        private const string ActionMapMenu = "MenuControls";
        
        private void EnableFirstPlayerController()
        {
            playerController2.DeactivatePlayer();
            playerController1.ActivatePlayer();
            isFirstPlayerControllerActive = true;
            OnSwitchPlayerController(PlayerControllerIndex.First);
        }

        private void TogglePlayerController()
        {
            if (isFirstPlayerControllerActive)
            {
                playerController1.DeactivatePlayer();
                playerController2.ActivatePlayer();
                OnSwitchPlayerController(PlayerControllerIndex.Second);
            }
            else
            {
                playerController2.DeactivatePlayer();
                playerController1.ActivatePlayer();
                OnSwitchPlayerController(PlayerControllerIndex.First);
            }
            isFirstPlayerControllerActive = !isFirstPlayerControllerActive;
        }

        private void Awake()
        {
            _switchAvatarAction = playerInput.currentActionMap["SwitchAvatar"];
        }

        private void Start()
        {
            EnableFirstPlayerController();
            EnableGameplayControls();
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
    
        private void HandleSwitchAvatar(InputAction.CallbackContext context)
        {
            TogglePlayerController();
        }

        private void HandleControlsChanged(PlayerInput _playerInput)
        {
            Debug.Log($"ControlsChanged {playerInput.currentControlScheme}");
            //TODO: Controls change dynamically
        }

        private void HandleActionTriggered(InputAction.CallbackContext context)
        {
            //Debug.Log($"ActionTriggered {context.action.name} phase: {context.action.phase}");
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
        
        private void EnableGameplayControls()
        {
            playerInput.currentActionMap.Disable();
            playerInput.SwitchCurrentActionMap(ActionMapGameplay);
            playerInput.currentActionMap.Enable();
        }
        
        private void EnablePauseMenuControls()
        {
            playerInput.currentActionMap.Disable();
            playerInput.SwitchCurrentActionMap(ActionMapMenu);
            playerInput.currentActionMap.Enable();
        }
    
    }
}
