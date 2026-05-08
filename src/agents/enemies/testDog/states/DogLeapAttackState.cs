using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class DogLeapAttackState : EnemyAtomicState
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
        Dog.NavigationAgent3D.AvoidanceEnabled = false;
        Dog.ShouldLeapAttackShapeCast.Enabled = false;
        Dog.StartAttacking();
        Dog.inCombatStateMachine.Travel("leapAttackStart");
    }

    public override void StateExited()
    {
        base.StateExited();
        Dog.leapAttacking = false;
        Dog.nextAttackIsLeap = false;
        Dog.ShouldLeapAttackShapeCast.Enabled = false;
        Dog.ShouldCloseAttackShapeCast.Enabled = false;
        Dog.NavigationAgent3D.AvoidanceEnabled = true;
        Dog.StopAttacking();
        Dog.AttackRateTimer.Start();
        Dog.ResetCurrentAiComponent();
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (CombatAgent is null) return;
        
        if (CombatAgent.ragdoll || CombatAgent.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RagdollState"));
            return;
        }
        
        if (CombatAgent.Staggered)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("StaggeredState"));
            return;
        }

        if (!Dog.Attacking)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
