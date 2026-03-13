using UnityEngine;

/// <summary>
/// Player State Machine
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReader input;
    [SerializeField] private PlayerMovement motor;
    [SerializeField] private PlayerInteract interact;
    [SerializeField] private PlayerCamera cam;

    [SerializeField] private PlayerDEBUG debug;

    // ACCESSORS ---
    public PlayerMovement Motor => motor;
    public PlayerCamera Camera => cam;
    public PlayerInteract Interact => interact;
    public PlayerDEBUG Debug => debug;

    private PlayerState _currentState;

    void Start() {
        // begin the game in default state
        SwitchState(new DefaultState(this, input));
    }

    // TODO: check this later
    public void SwitchState(PlayerState newState) {
        _currentState?.Exit();  // cleanup old state
        _currentState = newState;
        _currentState.Enter();
    }

    void Update() {
        _currentState?.Update();
    }
}
