using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerMovingState : PlayerAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.InputDirections.LengthSquared() == 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerIdleState"));
        }
    }
}
