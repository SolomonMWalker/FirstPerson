using FirstPerson.CustomTypes.StateMachine;

public partial class GruntNoActionState : NoActionState
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
        stateMachine.Travel("idle");
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Grunt.readyToFire)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("AimingState"));
            return;
        }
    }
}
