using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;
using Godot;

namespace FirstPerson.Scenes.Player.playerState;

public partial class PlayerJumpingState : PlayerAtomicState
{
    [Export] public Timer DontSwitchToGroundedEarlyTimer { get; set; }
    public override void StateEntered()
    {
        base.StateEntered();
        DontSwitchToGroundedEarlyTimer.Start();
        PlayerController.Jump();
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
        
        if (DontSwitchToGroundedEarlyTimer.IsStopped())
        {
            if(PlayerController.IsOnFloor())
            {
                if (PlayerController.CheckFallSpeed())
                {
                    PlayerController.CameraEffects.AddFallKick(2.0f);
                }
                OnStateChangeRequired(new ChangeStateEventArgs("PlayerGroundedState"));
                return;
            }
        }
        
        PlayerController.CurrentFallVelocity = PlayerController.Velocity.Y;
    }
}