public partial class DogStaggeredState : StaggeredState
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
        if (Dog is null) return;
        if (Dog.inCombat)
            Dog.inCombatStateMachine.Travel("stagger");
        else
            Dog.notInCombatStateMachine.Travel("staggered");
    }
}
