using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class FollowState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.behaviorState = Grunt.BehaviorState.Following;
        Grunt.FireRateTimer.Start();
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null || Grunt.firing) return;

        if (Grunt.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("DeadState"));
            return;
        }
        
        Grunt.AgentFollowComponent.HandleNavigation(delta);
    }
}
