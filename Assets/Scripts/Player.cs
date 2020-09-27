using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    
    private void Start()
    {
        Debug.Log($"Player index {playerInput.playerIndex} OnStart");
        var map = playerInput.currentActionMap;
    }

    private void OnEnable()
    {
        playerInput.onDeviceLost += HandleDeviceLost;
        playerInput.onDeviceRegained += HandleDeviceRegained;
        playerInput.onActionTriggered += HandleActionTriggered;
        playerInput.onControlsChanged += HandleControlsChanged;
    }
    
    private void OnDisable()
    {
        playerInput.onDeviceLost -= HandleDeviceLost;
        playerInput.onDeviceRegained -= HandleDeviceRegained;
        playerInput.onActionTriggered -= HandleActionTriggered;
        playerInput.onControlsChanged -= HandleControlsChanged;
    }

    private void HandleControlsChanged(PlayerInput obj)
    {
        Debug.Log($"ControlsChanged {playerInput.currentControlScheme}");
        //TODO: Controls change dynamically
    }

    private void HandleActionTriggered(InputAction.CallbackContext context)
    {
        Debug.Log($"ActionTriggered {context.action.name} phase: {context.action.phase}");

        switch (context.action)
        {
            
        }
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

    
    // onActionTriggered (collective event for all actions on the player)
    // onDeviceLost
    // onDeviceRegained
}
