using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;

public partial class CrouchingState : BasePlayerAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Player.CrouchingCollisionShape.SetDisabled(false);
        Player.StandingCollisionShape.SetDisabled(true);
        Player.ClamberController.Position = Player.ClamberController.CrouchingLocation.Position;
        Player.CameraController.EnterCrouchTweenActivate();
    }

    public override void StateExited()
    {
        base.StateExited();
        Player.StandingCollisionShape.SetDisabled(false);
        Player.CrouchingCollisionShape.SetDisabled(true);
        Player.Crouching = false;
        Player.ClamberController.Position = Player.ClamberController.StandingLocation.Position;
        Player.CameraController.ExitCrouchTweenActivate();
        
    }
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Player.Sprinting)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("CrouchingState"));
        }
        else if (!Player.Crouching)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WalkingState"));
        }
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        Player.CameraController.UpdateCameraHeight(delta, -1);
    }
}
