using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class FiringState : EnemyAtomicState
{
    public override void StateExited()
    {
        base.StateExited();
        Grunt.CustomAnimationTree.TrySetParam("doneFiring", true);
        Grunt.FuzzyStartTimer.FuzzyStart();
        Grunt.freezeRotation = false;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null) return;
        
        if (Grunt.ragdoll || Grunt.dead)
        {
            Grunt.AnimationPlayer.Stop();
            OnStateChangeRequired(new ChangeStateEventArgs("RagdollState"));
            return;
        }
        
        if(!Grunt.IsOnFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
            return;
        }

        if (Grunt.Staggered)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("StaggeredState"));
            return;
        }
        
        if (!Grunt.firing)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
        
    }
}
