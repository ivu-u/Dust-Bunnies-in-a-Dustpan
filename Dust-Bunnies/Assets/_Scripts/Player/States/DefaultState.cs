using UnityEngine;
using UnityEngine.Windows;

/// <summary>
/// Player movements in the overworld
/// </summary>
public class DefaultState : PlayerState
{
    public DefaultState(PlayerController p, InputReader i) : base(p, i) { }

    public override void Enter() {
        base.Enter();

        input.SwitchMaps(input.Maps.Default);
        input.OnInteractPerformed += OnInteract;
        input.OnNextScenePerformed += NextScene;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // TODO: this is called by PlayerController.SwitchState()
    // but i don't think that's clean
    public override void Exit() {
        input.OnInteractPerformed -= OnInteract;
        input.OnNextScenePerformed -= NextScene;

        base.Exit();
    }

    public override void Update() {
        base.Update();
        player.Motor.Move(input.Move);
        player.Camera.CameraMovement(input.Look);
        player.Camera.CamFree(input.Look);
    }

    private void OnInteract() {
        Interactable obj = player.Interact.TryPickUp();
        if (obj == null) return;

        player.SwitchState(new InteractState(player, input));
    }

    // DEBUG
    private void NextScene() {
        input.Maps.Disable();           // TODO: bad practice. should not have direct access
        player.Debug.NextScene();
    }
}
