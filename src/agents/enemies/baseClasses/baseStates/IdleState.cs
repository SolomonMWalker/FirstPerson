using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;
using Godot;

public partial class IdleState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        //CombatAgent.SetCurrentNavComponent(CombatAgent.AgentIdleComponent);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (CombatAgent is null || CombatAgent.CanChangeState()) return;
        if (CombatAgent.EnemyStateMachine.CurrentCombatState == "InCombatState")
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FollowState"));
        }
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (CombatAgent is null) return;
        
        if (CombatAgent.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("DeadState"));
            return;
        }
    }
}
