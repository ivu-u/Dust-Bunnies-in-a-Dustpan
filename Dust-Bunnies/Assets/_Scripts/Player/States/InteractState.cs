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
        input.OnRotatePerformed += RotateObject;

        player.Interact.PickUpObj();
    }

    public override void Exit() {
        base.Exit();
        input.OnRotatePerformed -= RotateObject;
    }

    public override void Update() {
        base.Update();
    }


    private void RotateObject(float value) {

    }
}
