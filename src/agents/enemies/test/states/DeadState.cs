using Godot;
using System;
using FirstPerson.scenes.enemies.test.states;

public partial class DeadState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt.CharacterBody3D.Velocity = Vector3.Zero;
        Grunt.NavigationAgent3D.Velocity = Vector3.Zero;
    }
}
