using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class PlayerWalkingState : PlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        PlayerController.CurrentMovementMult = PlayerController.DefaultMovementMult;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);

        var airborneState = PlayerStateMachine.GetAirborneState();

        if (Input.IsActionJustPressed("Crouch")
            && airborneState is "PlayerGroundedState"
            && !PlayerController.CameraController.AreCrouchTweensRunning())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerCrouchingState"));
            return;
        }

        if (Input.IsActionPressed("Sprint")
            && PlayerController.InputDirections.LengthSquared() > 0
            && airborneState is "PlayerGroundedState")
        {
            OnStateChangeRequired(new ChangeStateEventArgs("PlayerSprintingState"));
            return;
        }
    }
}
