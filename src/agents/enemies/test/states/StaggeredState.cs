using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class StaggeredState : EnemyAtomicState
{
    
    public override void StateEntered()
    {
        base.StateEntered();
        if (Grunt is null) return;
        Grunt.CustomAnimationTree.TrySetParam("notStaggered", false);
        Grunt.CustomAnimationTree.TrySetParam("staggered", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        if (Grunt is null) return;
        Grunt.FireRateTimer.Start();
        Grunt.CustomAnimationTree.TrySetParam("notStaggered", true);
        Grunt.CustomAnimationTree.TrySetParam("staggered", false);
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
        
        if(!Grunt.IsFloorRaycastColliding())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
            return;
        }
        
        if (!Grunt.staggered)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
