using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerCoyoteTimeState : PlayerAtomicState
{
    [Export] public Timer Timer { get; set; }
    
    public override void StateEntered()
    {
        base.StateEntered();
        Player.InAir = true;
        Timer.Start();
    }

    public override void StateExited()
    {
        base.StateExited();
        Timer.Stop();
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);

        if (Input.IsActionJustPressed("Jump"))
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerJumpingState"));
            return;
        }
        
        if (Player.IsOnFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerGroundedState"));
            return;
        }
        
        if (Timer.IsStopped())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerDefaultInAirState"));
            return;
        }
    }
}
