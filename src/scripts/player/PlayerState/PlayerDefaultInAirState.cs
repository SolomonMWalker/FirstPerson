using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerDefaultInAirState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        PlayerController.InAir = true;
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        
        if (Input.IsActionPressed("Jump") && PlayerController.ClamberController.TryHandleClamber())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerClamberingState"));
            return;
        }
        
        if(PlayerController.IsOnFloor())
        {
            if (PlayerController.CheckFallSpeed())
            {
                PlayerController.CameraEffects.AddFallKick(2.0f);
            }
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerGroundedState"));
        }

        PlayerController.CurrentFallVelocity = PlayerController.Velocity.Y;
    }
}
