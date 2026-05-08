using FirstPerson.CustomTypes.StateMachine;
using Godot;

public partial class DogNoActionState : NoActionState
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
        sm.Travel("idle");
    }
    
    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Dog.meleeAttacking)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("MeleeAttackState"));
            return;
        }

        if (Dog.leapAttacking)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("LeapAttackState"));
            return;
        }
    }
}
