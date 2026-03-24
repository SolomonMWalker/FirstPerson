using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class StaggeredState : EnemyAtomicState
{
    
    public override void StateEntered()
    {
        base.StateEntered();
        if (CombatAgent is null) return;
        //CombatAgent.CustomAnimationTree.TrySetParam("notStaggered", false);
        //CombatAgent.CustomAnimationTree.TrySetParam("staggered", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        if (CombatAgent is null) return;
        // CombatAgent.FireRateTimer.FuzzyStart();
        // CombatAgent.CustomAnimationTree.TrySetParam("notStaggered", true);
        // CombatAgent.CustomAnimationTree.TrySetParam("staggered", false);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (CombatAgent is null) return;
        
        if (CombatAgent.ragdoll || CombatAgent.dead)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RagdollState"));
            return;
        }
        
        if(!CombatAgent.IsFloorRaycastColliding())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("FallingState"));
            return;
        }
        
        if (!CombatAgent.Staggered)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("NoActionState"));
            return;
        }
    }
}
