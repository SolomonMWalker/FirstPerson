using Godot;
using System;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class FallingState : EnemyAtomicState
{
    private bool _doneFalling;
    
    public override void StateEntered()
    {
        base.StateEntered();
        _doneFalling = false;
        Grunt.falling = true;
        // Grunt.AnimationPlayer.Play(Grunt.IdleToFalling);
        // Grunt.AnimationPlayer.Queue(Grunt.Falling);
        Grunt.CustomAnimationTree.TrySetParam("notFalling", false);        
        Grunt.CustomAnimationTree.TrySetParam("falling", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.falling = false;
        Grunt.ApplyFloorSnap();
        Grunt.CustomAnimationTree.TrySetParam("notFalling", true);        
        Grunt.CustomAnimationTree.TrySetParam("falling", false);
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (!_doneFalling && Grunt.IsFloorRaycastColliding())
        {
            _doneFalling = true;
            Grunt.AnimationPlayer.Play(Grunt.FallingToIdle);
            return;
        }

        if (_doneFalling && Grunt.AnimationPlayer.IsPlaying())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
