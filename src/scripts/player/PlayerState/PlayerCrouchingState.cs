using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerCrouchingState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.InAir = true;
        Player.CurrentMovementMult = Player.CrouchMovementMult;
        Player.CrouchingCollisionShape.SetDisabled(false);
        Player.StandingCollisionShape.SetDisabled(true);
        Player.CameraController.EnterCrouchTweenActivate();
    }

    public override void StateExited()
    {
        base.StateExited();
        Player.StandingCollisionShape.SetDisabled(false);
        Player.CrouchingCollisionShape.SetDisabled(true);
        Player.CameraController.ExitCrouchTweenActivate();
    }
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        
        var airborneState = PlayerStateMachine.GetAirborneState();
        
        if (Input.IsActionPressed("Sprint")
            && Player.InputDirections.LengthSquared() > 0
            && airborneState is "PlayerGroundedState")
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerSprintingState"));
            return;
        }
        
        if (Input.IsActionJustPressed("Crouch")
                 && !Player.CameraController.AreCrouchTweensRunning()
                 && airborneState is "PlayerGroundedState")
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerWalkingState"));
            return;
        }
    }
}
