using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class GruntFiringState : EnemyAtomicState
{
    private Grunt Grunt { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        Grunt = (Grunt)CombatAgent;
    }
    
    public override void StateExited()
    {
        base.StateExited();
        Grunt.FireRateTimer.FuzzyStart();
        CombatAgent.freezeRotation = false;
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        
        if (Grunt.dead || Grunt.ragdoll) return;

        if (Grunt.Staggered)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("StaggeredState"));
        }
        
        if (!Grunt.firing)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
        
    }
}
