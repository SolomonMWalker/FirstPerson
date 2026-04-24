using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class GruntAimingState : EnemyAtomicState
{
    private Grunt Grunt { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        Grunt = (Grunt)CombatAgent;
    }

    public override void StateEntered()
    {
        base.StateEntered();
        if (Grunt is null) return;
        Grunt.readyToFire = false;
        Grunt.firing = true;
        Grunt.aimingOver = false;
        Grunt.CustomAnimationTree.TrySetParam("doneFiring", false);
        Grunt.CustomAnimationTree.TrySetParam("aiming", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.CustomAnimationTree.TrySetParam("aiming", false);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);

        if (Grunt.dead || Grunt.ragdoll) return;

        if (Grunt.Staggered)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("StaggeredState"));
        }
        
        if (Grunt.aimingOver)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FiringState"));
            return;
        }
    }
}
