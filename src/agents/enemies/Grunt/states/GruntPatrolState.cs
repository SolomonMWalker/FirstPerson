using FirstPerson.scenes.enemies.test.states;

public partial class GruntPatrolState : EnemyAtomicState
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
        Grunt.SetCurrentNavComponent(Grunt.AgentPatrolComponent);
    }
}
