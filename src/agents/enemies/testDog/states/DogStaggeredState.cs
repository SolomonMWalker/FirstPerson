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
        Dog.CustomAnimationTree.TrySetParam("notStaggered", false);
        Dog.CustomAnimationTree.TrySetParam("staggered", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        if (Dog is null) return;
        Dog.CustomAnimationTree.TrySetParam("notStaggered", true);
        Dog.CustomAnimationTree.TrySetParam("staggered", false);
    }
}
