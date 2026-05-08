using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class DogMeleeAttackState : EnemyAtomicState
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
        Dog.ShouldCloseAttackShapeCast.Enabled = false;
        Dog.StartAttacking();
        Dog.inCombatStateMachine.Travel("stationaryAttack");
    }

    public override void StateExited()
    {
        base.StateExited();
        Dog.meleeAttacking = false;
        Dog.nextAttackIsLeap = true;
        Dog.ShouldCloseAttackShapeCast.Enabled = false;
        Dog.ShouldLeapAttackShapeCast.Enabled = false;
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
        
        if(!CombatAgent.IsOnFloor())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
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
