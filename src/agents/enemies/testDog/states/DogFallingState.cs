public partial class DogFallingState : FallingState
{
    private Dog Dog { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        Dog = (Dog)CombatAgent;
    }
    
    public override void StateEntered()
    {
        base.StateEntered();
        var sm = Dog.inCombat ? Dog.inCombatStateMachine : Dog.notInCombatStateMachine;
        sm.Travel("falling");
    }
}
