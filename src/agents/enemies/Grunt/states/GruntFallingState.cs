using Godot;

public partial class GruntFallingState : FallingState
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
        var stateMachine = Grunt.inCombat ? Grunt.inCombatStateMachine : Grunt.notInCombatStateMachine;
        stateMachine.Travel("falling");
    }
}
