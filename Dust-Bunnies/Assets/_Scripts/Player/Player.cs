using UnityEngine;
using UnityEngine.InputSystem;

// TODO: see if it makes sense to have the player classes as partial classes
/// <summary>
/// Hub for player input routing
/// </summary>
public class Player : MonoBehaviour {
    public enum PlayerState {   // wip (not sure if we need this)
        Default,
        Crouch,
        Menu,
        Minigame,
    }

    public event System.Action OnInteractPerformed;
    public event System.Action<float> OnRotatePerformed;

    private PlayerInputs _playerInput;

    public Vector3 MovementVector {
        get { return _playerInput.Default.Move.ReadValue<Vector2>();  }
    }

    void Awake() {
        _playerInput = new();
        _playerInput.Enable();    // should probably move this at some point 
    }

    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    #region Subscribing to Events
    private void OnEnable() {
        ActivateDefaultInputs();
        _playerInput.Default.Interact.performed += InteractPerformed;
        _playerInput.Interact.Rotate.performed += RotatePerformed;
    }

    private void OnDisable() {
        DeactivateInputs();
        _playerInput.Default.Interact.performed -= InteractPerformed;
        _playerInput.Interact.Rotate.performed -= RotatePerformed;

    }
    #endregion

    public void ActivateDefaultInputs() {
        _playerInput.Default.Enable();
        // will have to find a way to disable the rest. Maybe a system that tracks when you switch
    }

    /// <summary>
    /// When the player picks up an object to inspect it.
    /// </summary>
    public void ActivateInteractInputs() {
        Debug.Log("Interact inputs activated");
        DeactivateInputs();     // IDK IF DEACTIVATING ALL INPUTS IS GOOD PRACTICE BUT IF IT WORKS  
        _playerInput.Interact.Enable();
        // NOTE: Controls for rotating object (locked cursor) click and drag mouse)
        // TEMP CONTROLS (testing): simple axis for left and right rotation
    }

    /// <summary>
    /// This is def a jank way to do this
    /// </summary>
    public void DeactivateInputs() {
        _playerInput.Default.Disable();
        _playerInput.Interact.Disable();
    }

    /// --- PERFORMED METHODS ---

    private void InteractPerformed(InputAction.CallbackContext context) {
        if (context.performed) OnInteractPerformed?.Invoke();
    }

    private void RotatePerformed(InputAction.CallbackContext context) {
        if (context.performed) OnRotatePerformed?.Invoke(context.ReadValue<float>());
    }
}
