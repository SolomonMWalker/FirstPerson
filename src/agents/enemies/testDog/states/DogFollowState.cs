public partial class DogFollowState : FollowState
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
        Dog.SetCurrentNavComponent(Dog.AgentFollowComponent);
    }
}
