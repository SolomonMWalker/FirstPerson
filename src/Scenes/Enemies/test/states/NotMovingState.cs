using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class NotMovingState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        Grunt?.AnimationPlayer.Play("idle");
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;
        if (Grunt.CharacterBody3D.Velocity.LengthSquared() > 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("IsMovingState"));
        }
    }
}
