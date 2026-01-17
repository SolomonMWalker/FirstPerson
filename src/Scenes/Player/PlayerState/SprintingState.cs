using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class SprintingState : BasePlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.Crouching = false;
        Player.PlayEnterSprintAnim();
    }

    public override void StateExited()
    {
        base.StateExited();
        Player.Sprinting = false;
        Player.PlayExitSprintAnim();
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.InputDirections.LengthSquared() == 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WalkingState"));
        }
        else if (Player.Crouching)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("CrouchingState"));
        }
    }
}
