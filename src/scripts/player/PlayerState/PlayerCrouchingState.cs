using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerCrouchingState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        PlayerController.InAir = true;
        PlayerController.CurrentMovementMult = PlayerController.CrouchMovementMult;
        PlayerController.CrouchingCollisionShape.SetDisabled(false);
        PlayerController.StandingCollisionShape.SetDisabled(true);
        PlayerController.CameraController.EnterCrouchTweenActivate();
    }

    public override void StateExited()
    {
        base.StateExited();
        PlayerController.StandingCollisionShape.SetDisabled(false);
        PlayerController.CrouchingCollisionShape.SetDisabled(true);
        PlayerController.CameraController.ExitCrouchTweenActivate();
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
