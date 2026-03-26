public partial class GruntIdleState : IdleState
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
        Grunt.SetCurrentNavComponent(Grunt.AgentIdleComponent);
    }
}
