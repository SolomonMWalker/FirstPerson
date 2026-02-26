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
        GD.Print("started falling");
        _doneFalling = false;
        Grunt.falling = true;
        //Grunt.AnimationPlayer.Play(Grunt.IdleToFalling);
        //Grunt.AnimationPlayer.Queue(Grunt.Falling);
    }

    public override void StateExited()
    {
        base.StateExited();
        GD.Print("stopped falling");
        Grunt.falling = false;
        Grunt.ApplyFloorSnap();
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (!_doneFalling && Grunt.IsOnFloor())
        {
            _doneFalling = true;
            //Grunt.AnimationPlayer.Play(Grunt.FallingToIdle);
            //return;
        }

        //if (_doneFalling && Grunt.AnimationPlayer.IsPlaying())
        if(_doneFalling)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
