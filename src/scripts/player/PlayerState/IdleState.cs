using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class IdleState : BasePlayerAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.Clambering)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("ClamberingState"));
        }
        if (Player.InputDirections.LengthSquared() > 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("MovingState"));
        }
    }
}
