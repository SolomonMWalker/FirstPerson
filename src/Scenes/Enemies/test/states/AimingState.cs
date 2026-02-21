using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class AimingState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        if (Grunt is null) return;
        Grunt.readyToFire = false;
        Grunt.firing = true;
        Grunt.AnimationPlayer.Play(Grunt.Aim);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null) return;
        Grunt.RotateToTarget();
        if (!Grunt.AnimationPlayer.IsPlaying())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FiringState"));
        }
        
    }
}
