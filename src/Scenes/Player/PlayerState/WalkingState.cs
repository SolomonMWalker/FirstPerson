using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class WalkingState : BasePlayerAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.Sprinting)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("SprintingState"));
        }
    }
}
