using FirstPerson.CustomTypes.StateMachine;

public partial class GruntIsMovingState : IsMovingState
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
        Grunt.CustomAnimationTree.TrySetParam("moving", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Grunt.CustomAnimationTree.TrySetParam("moving", false);
    }

    public override void StatePhysicsProcessing(double delta)
    {
        if (Grunt.readyToFire)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("AimingState"));
            return;
        }
    }
}
