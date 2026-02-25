using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerCrouchingState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        //PlayerController.InAir = true;
        PlayerController.StartCrouch();
    }

    public override void StateExited()
    {
        base.StateExited();
       PlayerController.EndCrouch();
    }
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        
        var airborneState = PlayerStateMachine.GetAirborneState();
        
        if (Input.IsActionPressed("Sprint")
            && PlayerController.InputDirections.LengthSquared() > 0
            && airborneState is "PlayerGroundedState")
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerSprintingState"));
            return;
        }
        
        if (Input.IsActionJustPressed("Crouch")
                 && !PlayerController.CameraController.AreCrouchTweensRunning()
                 && airborneState is "PlayerGroundedState")
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerWalkingState"));
            return;
        }
    }
}
