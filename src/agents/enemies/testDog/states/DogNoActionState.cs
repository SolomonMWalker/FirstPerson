using FirstPerson.CustomTypes.StateMachine;

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
        Dog.CustomAnimationTree.TrySetParam("stopped", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Dog.CustomAnimationTree.TrySetParam("stopped", false);
    }
    
    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (Dog.meleeAttacking)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("MeleeAttackState"));
            return;
        }
    }
}
