using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerClamberingState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.Clambering = true;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (!Player.Clambering)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerIdleState"));
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerDefaultInAirState"));
        }
    }
}
