using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class FiringState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        GD.Print("entering firing state");
        if (Grunt is null) return;
        Grunt.AnimationPlayer.Play(Grunt.FireAnimation);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.FireRateTimer.Start();
        Grunt.firing = false;
        Grunt.freezeRotation = false;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        
        if (Grunt.ragdoll || Grunt.dead)
        {
            Grunt.AnimationPlayer.Stop();
            OnStateChangeRequired(new ChangeStateEventArgs("RagdollState"));
            return;
        }
        
        if(!Grunt.ShouldSnapToFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
            return;
        }
        
        if (Grunt is null) return;
        if (!Grunt.AnimationPlayer.IsPlaying())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
        
    }
}
