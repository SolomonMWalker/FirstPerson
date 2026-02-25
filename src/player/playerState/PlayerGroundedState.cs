using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerGroundedState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        PlayerController.InAir = false;
        PlayerController.Velocity = PlayerController.Velocity with { Y = 0 };
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Input.IsActionJustPressed("Jump"))
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerJumpingState"));
            return;
        }
        if (!PlayerController.IsOnFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerCoyoteTimeState"));
            return;
        }
    }
}
