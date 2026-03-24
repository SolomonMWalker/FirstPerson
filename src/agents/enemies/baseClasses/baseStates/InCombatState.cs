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
        //CombatAgent.CustomAnimationTree.TrySetParam("inCombat", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        //CombatAgent.CustomAnimationTree.TrySetParam("inCombat", false);
    }
}
