using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// All Inputs routed here
/// </summary>
public class InputReader : MonoBehaviour
{
    private InputActionMaps _inputs;

    // CONTINUOUS INPUTS    ---
    public Vector2 Move => _inputs.Default.Move.ReadValue<Vector2>();
    public Vector2 Look =>  _inputs.Default.Look.ReadValue<Vector2>();
    public Vector2 Rotate => _inputs.Interact.Rotate.ReadValue<Vector2>();
    // sprint

    // EVENTS   ---
    public event System.Action OnInteractPerformed;
    public event System.Action OnInteractExit;
    public event System.Action OnRotatePerformed;
    public event System.Action OnRotateExit;

    // DEBUG
    public event System.Action OnNextScenePerformed;

    // OTHER    ---
    public InputActionMaps Maps => _inputs;       // TODO: bad practice
    private InputActionMap currentMap;

    void Awake() {
        _inputs = new InputActionMaps();
    }

    void OnEnable() {
        _inputs.Default.Interact.performed += InteractPerformed;
        _inputs.Interact.Return.performed += InteractExitPerformed;
        _inputs.Interact.StartRotate.performed += RotatePerformed;
        _inputs.Interact.StartRotate.canceled += RotatePerformed;

        // DEBUG
        _inputs.DEBUG.NextScene.performed += NextScenePerformed;
    }

    void OnDisable() {
        _inputs.Default.Interact.performed -= InteractPerformed;
        _inputs.Interact.Return.performed -= InteractExitPerformed;
        _inputs.Interact.StartRotate.performed -= RotatePerformed;
        _inputs.Interact.StartRotate.canceled -= RotatePerformed;

        // DEBUG
        _inputs.DEBUG.NextScene.performed -= NextScenePerformed;

        _inputs.Dispose();
    }

    public void SwitchMaps(InputActionMap newMap) {
        // DEBUG --
        if (currentMap != null) {
            if (currentMap.name == "Default")
                Maps.DEBUG.Disable();
        } else if (newMap.name == "Default") {
            Maps.DEBUG.Enable();
        }
            

        currentMap?.Disable();
        currentMap = newMap;
        currentMap.Enable();
    }

    private void InteractPerformed(InputAction.CallbackContext _) => 
        OnInteractPerformed?.Invoke();

    private void InteractExitPerformed(InputAction.CallbackContext _) =>
        OnInteractExit?.Invoke();

    private void RotatePerformed(InputAction.CallbackContext context) {
        if (context.performed) OnRotatePerformed?.Invoke();
        else if (context.canceled) OnRotateExit?.Invoke();
    }

    private void NextScenePerformed(InputAction.CallbackContext _) =>
        OnNextScenePerformed?.Invoke();
}
