using Godot;
using System;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class NotInCombatState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        CombatAgent.inCombat = false;
        OnStateChangeRequired(new ChangeStateEventArgs(CombatAgent.DefaultNonCombatBehaviorStateName));
        CombatAgent.ResetCurrentAiComponent();
    }
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (CombatAgent is null || !CombatAgent.CanChangeState()) return;

        if (CombatAgent.inCombat && CombatAgent.MovementTarget is not null)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("InCombatState"));
            return;
        }
    }
}
