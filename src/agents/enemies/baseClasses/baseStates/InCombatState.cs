using Godot;
using System;
using System.Linq;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class InCombatState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        CombatAgent.inCombat = true;
        OnStateChangeRequired(new ChangeStateEventArgs(CombatAgent.DefaultCombatBehaviorStateName));
        CombatAgent.ResetCurrentAiComponent();
    }
}
