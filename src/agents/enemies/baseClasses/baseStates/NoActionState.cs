using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class NoActionState : EnemyAtomicState
{
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
        
        if (!CombatAgent.Velocity.IsZeroApprox())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("IsMovingState"));
            return;
        }
    }
}
