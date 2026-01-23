using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class GroundedState : BasePlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.InAir = false;
        Player.Velocity = Player.Velocity with { Y = 0 };
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Input.IsActionJustPressed("Jump"))
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerJumpingState"));
            return;
        }
        if (!Player.IsOnFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("CoyoteTimeState"));
            return;
        }
    }
}
