using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    public enum PlayerState {   // wip (not sure if we need this)
        Default,
        Crouch,
        Menu,
        Minigame,
    }

    public event System.Action OnInteractPerformed;

    private PlayerInputs _playerInputs;

    void Awake() {
        _playerInputs = new();
        _playerInputs.Enable();    // should probably move this at some point 
    }

    private void OnEnable() {
        ActivateDefaultInputs();
        _playerInputs.Default.Interact.performed += InteractPerformed;
    }

    public void ActivateDefaultInputs() {
        _playerInputs.Default.Enable();
        // will have to find a way to disable the rest. Maybe a system that tracks when you switch
    }

    private void InteractPerformed(InputAction.CallbackContext context) {
        if (context.performed) OnInteractPerformed?.Invoke();
    }
}
