using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Undercooked.Player
{
    public class InputController : MonoBehaviour
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

        private bool _isFirstPlayerControllerActive;
    
        private InputAction _switchAvatarAction;
        private InputAction _startAtMenuAction;
        private InputAction _startAtPlayerAction;
        
        private bool _hasSubscribedPlayerActions;
        private bool _hasSubscribedMenuActions;
        public delegate void SwitchPlayerController(PlayerControllerIndex playerControllerIndex);
        public static SwitchPlayerController OnSwitchPlayerController;

        private const string ActionMapGameplay = "PlayerControls";
        private const string ActionMapMenu = "MenuControls";

        public delegate void StartPressed();
        public StartPressed OnStartPressedAtMenu;
        public StartPressed OnStartPressedAtPlayer;

        private void Awake()
        {
            #if UNITY_EDITOR
            Assert.IsNotNull(playerInput);
            Assert.IsNotNull(playerController1);
            Assert.IsNotNull(playerController2);
            #endif
        }

        internal void EnableFirstPlayerController()
        {
            playerController2.DeactivatePlayer();
            playerController1.ActivatePlayer();
            _isFirstPlayerControllerActive = true;
            OnSwitchPlayerController(PlayerControllerIndex.First);
        }
        
        internal void EnableSecondPlayerController()
        {
            playerController1.DeactivatePlayer();
            playerController2.ActivatePlayer();
            _isFirstPlayerControllerActive = false;
            OnSwitchPlayerController(PlayerControllerIndex.Second);
        }

        internal void DisableAllPlayerControllers()
        {
            playerController1.DeactivatePlayer();
            playerController2.DeactivatePlayer();
            _isFirstPlayerControllerActive = false;
            UnsubscribePlayerActions();
        }

        private void TogglePlayerController()
        {
            if (_isFirstPlayerControllerActive)
            {
                EnableSecondPlayerController();
            }
            else
            {
                EnableFirstPlayerController();
            }
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
        }

        private void UnsubscribeInputEvents()
        {
            playerInput.onDeviceLost -= HandleDeviceLost;
            playerInput.onDeviceRegained -= HandleDeviceRegained;
            playerInput.onActionTriggered -= HandleActionTriggered;
            playerInput.onControlsChanged -= HandleControlsChanged;
        }

        private void SubscribePlayerActions()
        {
            if (_hasSubscribedPlayerActions) return;
            _hasSubscribedPlayerActions = true;
            
            _switchAvatarAction = playerInput.currentActionMap["SwitchAvatar"];
            _startAtPlayerAction = playerInput.currentActionMap["Start@Player"];
            _startAtPlayerAction.performed += HandleStartAtPLayer;
            _switchAvatarAction.performed += HandleSwitchAvatar; 
        }

        private void UnsubscribePlayerActions()
        {
            if (_hasSubscribedPlayerActions == false) return;
            _hasSubscribedPlayerActions = false; 
            _startAtPlayerAction.performed -= HandleStartAtPLayer;
            _switchAvatarAction.performed -= HandleSwitchAvatar;
        }
        
        private void SubscribeMenuActions()
        {
            if (_hasSubscribedMenuActions) return;
            _hasSubscribedMenuActions = true;
            
            _startAtMenuAction = playerInput.currentActionMap["Start@Menu"];
            _startAtMenuAction.performed += HandleStartAtMenu;
        }
        
        private void UnsubscribeMenuActions()
        {
            if (_hasSubscribedMenuActions == false) return;
            _hasSubscribedMenuActions = false;
            _startAtMenuAction.performed -= HandleStartAtMenu;
        }
        
        private void HandleStartAtPLayer(InputAction.CallbackContext context)
        {
            OnStartPressedAtPlayer?.Invoke();
        }
        
        private void HandleStartAtMenu(InputAction.CallbackContext context)
        {
            OnStartPressedAtMenu?.Invoke();
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
        
        internal void EnableGameplayControls()
        {
            Debug.Log("[InputController] Enable GamePlayControls");
            UnsubscribeMenuActions();
            playerInput.currentActionMap.Disable();
            playerInput.SwitchCurrentActionMap(ActionMapGameplay);
            SubscribePlayerActions();
            playerInput.currentActionMap.Enable();
        }
        
        internal void EnableMenuControls()
        {
            Debug.Log("[InputController] Enable MenuControls");
            UnsubscribePlayerActions();
            playerInput.currentActionMap.Disable();
            playerInput.SwitchCurrentActionMap(ActionMapMenu);
            SubscribeMenuActions();
            playerInput.currentActionMap.Enable();            
        }
    
    }
}
