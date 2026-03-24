public partial class GruntFollowState : FollowState
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
        Grunt.FireRateTimer.FuzzyStart();
        Grunt.SetCurrentNavComponent(Grunt.AgentFollowComponent);
    }
}
