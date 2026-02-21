using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class FiringState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        if (Grunt is null) return;
        Grunt.AnimationPlayer.Play(Grunt.Fire);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.FireRateTimer.Start();
        Grunt.firing = false;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Grunt is null) return;
        if (!Grunt.AnimationPlayer.IsPlaying())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
        }
        
    }
}
