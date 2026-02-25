using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class IsMovingState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        if (Grunt.behaviorState == Grunt.BehaviorState.Following)
        {
            Grunt?.AnimationPlayer.Play(Grunt.IdleGunReadyToWalkGunReadyAnimation);
            Grunt?.AnimationPlayer.Queue(Grunt.WalkGunReadyAnimation);
        }
        else //idle
        {
            Grunt?.AnimationPlayer.Play(Grunt.IdleGunDownToWalkGunDownAnimation);   
            Grunt?.AnimationPlayer.Queue(Grunt.WalkGunDownAnimation);
        }
    }
    
    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt is null) return;
        if (Grunt.readyToFire)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("AimingState"));
            return;
        }
        if (Grunt.CharacterBody3D.Velocity.LengthSquared() == 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
