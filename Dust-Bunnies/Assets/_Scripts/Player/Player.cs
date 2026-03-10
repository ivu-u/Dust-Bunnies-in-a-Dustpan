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
    public event System.Action<float> OnRotatePerformed;

    private PlayerInputs _playerInputs;

    void Awake() {
        _playerInputs = new();
        _playerInputs.Enable();    // should probably move this at some point 
    }

    #region Subscribing to Events
    private void OnEnable() {
        ActivateDefaultInputs();
        _playerInputs.Default.Interact.performed += InteractPerformed;
        _playerInputs.Interact.Rotate.performed += RotatePerformed;
    }

    private void OnDisable() {
        DeactivateInputs();
        _playerInputs.Default.Interact.performed -= InteractPerformed;
        _playerInputs.Interact.Rotate.performed -= RotatePerformed;

    }
    #endregion

    public void ActivateDefaultInputs() {
        _playerInputs.Default.Enable();
        // will have to find a way to disable the rest. Maybe a system that tracks when you switch
    }

    /// <summary>
    /// When the player picks up an object to inspect it.
    /// </summary>
    public void ActivateInteractInputs() {
        Debug.Log("Interact inputs activated");
        DeactivateInputs();     // IDK IF DEACTIVATING ALL INPUTS IS GOOD PRACTICE BUT IF IT WORKS  
        _playerInputs.Interact.Enable();
        // NOTE: Controls for rotating object (locked cursor) click and drag mouse)
        // TEMP CONTROLS (testing): simple axis for left and right rotation
    }

    /// <summary>
    /// This is def a jank way to do this
    /// </summary>
    public void DeactivateInputs() {
        _playerInputs.Default.Disable();
        _playerInputs.Interact.Disable();
    }

    /// --- PERFORMED METHODS ---

    private void InteractPerformed(InputAction.CallbackContext context) {
        if (context.performed) OnInteractPerformed?.Invoke();
    }

    private void RotatePerformed(InputAction.CallbackContext context) {
        if (context.performed) OnRotatePerformed?.Invoke(context.ReadValue<float>());
    }
}
