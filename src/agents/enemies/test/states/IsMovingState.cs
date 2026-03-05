using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class IsMovingState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.CustomAnimationTree.TrySetParam("moving", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.CustomAnimationTree.TrySetParam("moving", false);
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;
        
        if (Grunt.ragdoll || Grunt.dead)
        {
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
        
        if (Grunt.readyToFire)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("AimingState"));
            return;
        }
        
        if (Grunt.previousFrameVelocityLengthSquared == 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
