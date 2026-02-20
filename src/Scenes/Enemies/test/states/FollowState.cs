using Godot;
using System;
using FirstPerson.scenes.enemies.test.states;

public partial class FollowState : EnemyAtomicState
{
    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;
        Grunt.HandleNavigation(delta);
        Grunt.CharacterBody3D.MoveAndSlide();
    }
}
