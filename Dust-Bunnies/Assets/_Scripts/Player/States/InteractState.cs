using UnityEngine;

/// <summary>
/// When the player picks up an object
/// </summary>
public class InteractState : PlayerState
{
    public InteractState(PlayerController p, InputReader i) : base(p, i) { }

    public override void Enter() {
        base.Enter();

        input.SwitchMaps(input.Maps.Interact);
        input.OnInteractExit += ExitInteract;

        player.Interact.PickUpObj();
    }

    public override void Exit() {
        input.OnInteractExit -= ExitInteract;


        base.Exit();
    }

    public override void Update() {
        base.Update();
        player.Interact.Rotate(input.Rotate);
    }

    private void ExitInteract() {
        player.Interact.PutDownObj();
        player.SwitchState(new DefaultState(player, input));
    }
}
