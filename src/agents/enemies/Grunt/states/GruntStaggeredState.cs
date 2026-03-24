public partial class GruntStaggeredState : StaggeredState
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
        if (Grunt is null) return;
        Grunt.CustomAnimationTree.TrySetParam("notStaggered", false);
        Grunt.CustomAnimationTree.TrySetParam("staggered", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        if (Grunt is null) return;
        Grunt.FireRateTimer.FuzzyStart();
        Grunt.CustomAnimationTree.TrySetParam("notStaggered", true);
        Grunt.CustomAnimationTree.TrySetParam("staggered", false);
    }
}
