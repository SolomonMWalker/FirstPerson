using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

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
        Dog.StartAttacking();
        //Dog.CustomAnimationTree.TrySetParam("attackOver", false);
        Dog.CustomAnimationTree.TrySetParam("stationaryAttack", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        Dog.meleeAttacking = false;
        Dog.StopAttacking();
        //Dog.CustomAnimationTree.TrySetParam("attackOver", true);
        Dog.CustomAnimationTree.TrySetParam("stationaryAttack", false);
        Dog.AttackRateTimer.Start();
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
            OnStateChangeRequired(new ChangeStateEventArgs("IdleState"));
            return;
        }
    }
}
