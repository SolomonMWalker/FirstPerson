using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class WalkingState : BasePlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.ClamberController.Position = Player.ClamberController.StandingLocation.Position;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.Sprinting)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("SprintingState"));
        }
        else if (Player.Crouching)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("CrouchingState"));
        }
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        Player.CameraController.UpdateCameraHeight(delta, 1);
    }
}
