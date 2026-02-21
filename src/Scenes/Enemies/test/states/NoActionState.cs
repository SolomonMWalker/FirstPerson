using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class NoActionState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        if (Grunt.behaviorState == Grunt.BehaviorState.Following)
        {
            Grunt?.AnimationPlayer.Play(Grunt.IdleGunReady);
        }
        else //idle
        {
            Grunt?.AnimationPlayer.Play(Grunt.IdleGunDown);
        }
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;
        if (Grunt.readyToFire)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("AimingState"));
        }
        if (Grunt.CharacterBody3D.Velocity.LengthSquared() > 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("IsMovingState"));
        }
    }
}
