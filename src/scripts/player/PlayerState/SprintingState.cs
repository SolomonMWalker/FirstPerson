using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class SprintingState : BasePlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.CurrentMovementMult = Player.SprintMovementMult;
        Player.CameraController.EnterSprintTweenActivate();
    }

    public override void StateExited()
    {
        base.StateExited();
        Player.CameraController.ExitSprintTweenActivate();
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.InputDirections.LengthSquared() == 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WalkingState"));
            return;
        }
        
        var airborneState = PlayerStateMachine.GetAirborneState();
        
        if (Input.IsActionJustPressed("Crouch")
            && airborneState is "GroundedState"
            && !Player.CameraController.AreCrouchTweensRunning())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("CrouchingState"));
            return;
        }
    }
}
