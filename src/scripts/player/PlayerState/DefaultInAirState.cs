using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class DefaultInAirState : BasePlayerAtomicState
{
    [Export] public Timer Timer { get; set; }

    private bool EnteredFromJump;
    
    public override void StateEntered()
    {
        base.StateEntered();
        if (Player.Jumped)
        {
            Player.Jumped = false;
            EnteredFromJump = true;
            Timer.Start();
        }
    }

    public override void StateExited()
    {
        base.StateExited();
        EnteredFromJump = false;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (EnteredFromJump)
        {
            if (Timer.IsStopped() && Player.IsOnFloor())
            {
                OnStateChangeRequired(new ChangeStateEventArgs("GroundedState"));
            }
        }
        else if(Player.IsOnFloor())
        {
            if (Player.CheckFallSpeed())
            {
                Player.CameraEffects.AddFallKick(2.0f);
            }
            OnStateChangeRequired(new ChangeStateEventArgs("GroundedState"));
        }

        Player.CurrentFallVelocity = Player.Velocity.Y;
    }
}
