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
        Dog.CustomAnimationTree.TrySetParam("notFalling", false);        
        Dog.CustomAnimationTree.TrySetParam("falling", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Dog.CustomAnimationTree.TrySetParam("notFalling", true);        
        Dog.CustomAnimationTree.TrySetParam("falling", false);
    }
}
