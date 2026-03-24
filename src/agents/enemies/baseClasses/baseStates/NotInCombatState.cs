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
        //CombatAgent.CustomAnimationTree.TrySetParam("notInCombat", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        //CombatAgent.CustomAnimationTree.TrySetParam("notInCombat", false);
    }
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        //if (CombatAgent is null || CombatAgent.firing || CombatAgent.dead) return;
        if (CombatAgent.CombatTriggerArea.HasOverlappingAreas())
        {
            var overlapArea = CombatAgent.CombatTriggerArea.GetOverlappingAreas().First();
            if (overlapArea is Hitbox hitbox)
            {
                CombatAgent.CombatTarget = hitbox.Parent;
                OnStateChangeRequired(new ChangeStateEventArgs("InCombatState"));
                return;
            }
        }

        if (CombatAgent.inCombat && CombatAgent.CombatTarget is not null)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("InCombatState"));
            return;
        }
    }
}
