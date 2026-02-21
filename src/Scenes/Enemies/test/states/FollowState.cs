using Godot;
using System;
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
        Grunt.HandleNavigation(delta);
        Grunt.CharacterBody3D.MoveAndSlide();
    }
}
