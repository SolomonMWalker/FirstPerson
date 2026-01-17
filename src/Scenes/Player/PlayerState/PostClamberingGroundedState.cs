using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PostClamberingGroundedState : BasePlayerAtomicState
{
    [Export] public Timer Timer { get; set; }
    
    public override void StateEntered()
    {
        base.StateEntered();
        Timer.Start();
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Timer.IsStopped())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("GroundedState"));
        }
        else if (!Player.IsOnFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("CoyoteTimeState"));
        }
    }
}
