using FirstPerson.CustomTypes.StateMachine;
using Godot;

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
        var stateMachine = Grunt.inCombat ? Grunt.inCombatStateMachine : Grunt.notInCombatStateMachine;
        stateMachine.Travel("moving");
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
