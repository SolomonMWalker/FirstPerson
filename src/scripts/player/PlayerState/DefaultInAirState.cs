using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class DefaultInAirState : BasePlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.InAir = true;
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        
        if (Input.IsActionPressed("Jump") && Player.TryHandleClamber())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("ClamberingState"));
            return;
        }
        
        if(Player.IsOnFloor())
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
