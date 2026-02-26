using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class AimingState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        GD.Print("entering aiming state");
        if (Grunt is null) return;
        Grunt.readyToFire = false;
        Grunt.firing = true;
        Grunt.AnimationPlayer.Play(Grunt.AimAnimation);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null) return;
        
        if (Grunt.ragdoll || Grunt.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RagdollState"));
            return;
        }
        
        if(!Grunt.ShouldSnapToFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
            return;
        }
        
        if(Grunt.CanRotate()) Grunt.RotateToTarget();
        if (!Grunt.AnimationPlayer.IsPlaying())
        {
            GD.Print("change to firing state");
            OnStateChangeRequired(new ChangeStateEventArgs("FiringState"));
            return;
        }
        
    }
}
