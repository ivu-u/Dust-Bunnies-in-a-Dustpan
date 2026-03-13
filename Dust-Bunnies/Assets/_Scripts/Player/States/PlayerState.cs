using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;
    protected InputReader input;

    public PlayerState(PlayerController player, InputReader input) {
        this.player = player;
        this.input = input;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}
