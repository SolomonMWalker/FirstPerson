using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class FollowState : EnemyAtomicState
{
    public override void StatePhysicsProcessing(double delta)
    {
        base.StatePhysicsProcessing(delta);
        if (CombatAgent is null) return;

        if (CombatAgent.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("DeadState"));
            return;
        }

        if (CombatAgent.EnemyStateMachine.CurrentCombatState == "NotInCombatState"
            || CombatAgent.CombatTarget is null)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("IdleState"));
            return;
        }
    }
}
