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
        Grunt.CustomAnimationTree.TrySetParam("notFalling", false);        
        Grunt.CustomAnimationTree.TrySetParam("falling", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.CustomAnimationTree.TrySetParam("notFalling", true);        
        Grunt.CustomAnimationTree.TrySetParam("falling", false);
    }
}
