using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player.PlayerState;
using Godot;

namespace FirstPerson.Scenes.Player.playerState;

public partial class PlayerJumpingState : BasePlayerAtomicState
{
    [Export] public Timer DontSwitchToGroundedEarlyTimer { get; set; }
    public override void StateEntered()
    {
        base.StateEntered();
        DontSwitchToGroundedEarlyTimer.Start();
        Player.Jump();
        Player.InAir = true;
    }
    
    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);

        if (Input.IsActionPressed("Jump") && Player.TryHandleClamber())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("ClamberingState"));
            return;
        }
        
        if (DontSwitchToGroundedEarlyTimer.IsStopped())
        {
            if(Player.IsOnFloor())
            {
                if (Player.CheckFallSpeed())
                {
                    Player.CameraEffects.AddFallKick(2.0f);
                }
                OnStateChangeRequired(new ChangeStateEventArgs("GroundedState"));
                return;
            }
        }
        
        Player.CurrentFallVelocity = Player.Velocity.Y;
    }
}