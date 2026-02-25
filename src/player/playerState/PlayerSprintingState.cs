using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerSprintingState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        PlayerController.Sprinting = true;
        PlayerController.CurrentMovementMult = PlayerController.SprintMovementMult;
        PlayerController.CameraController.EnterSprintTweenActivate();
        if (PlayerController.WeaponController.Aiming)
        {
            PlayerController.WeaponController.Aiming = false;
        }
    }

    public override void StateExited()
    {
        base.StateExited();
        PlayerController.Sprinting = false;
        PlayerController.CameraController.ExitSprintTweenActivate();
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (PlayerController.InputDirections.LengthSquared() == 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerWalkingState"));
            return;
        }
        
        var airborneState = PlayerStateMachine.GetAirborneState();
        
        if (Input.IsActionJustPressed("Crouch")
            && airborneState is "PlayerGroundedState"
            && !PlayerController.CameraController.AreCrouchTweensRunning())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerCrouchingState"));
            return;
        }
    }
}
