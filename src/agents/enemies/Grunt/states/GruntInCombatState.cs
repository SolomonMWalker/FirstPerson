public partial class GruntInCombatState : InCombatState
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
        Grunt.combatStateSwitchStateMachine.Travel("InCombatStateMachine");
    }
}
