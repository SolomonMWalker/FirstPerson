public partial class GruntStaggeredState : StaggeredState
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
        Grunt?.inCombatStateMachine.Travel("staggered");
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt?.FireRateTimer.FuzzyStart();
    }
}
