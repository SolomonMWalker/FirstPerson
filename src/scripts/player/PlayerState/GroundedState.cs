using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class GroundedState : BasePlayerAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.Jumped)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("DefaultInAirState"));
        }
        else if (!Player.IsOnFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("CoyoteTimeState"));
        }
    }
}
