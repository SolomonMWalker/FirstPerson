using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.scenes.enemies.test.states;

public partial class NoActionState : EnemyAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        //CombatAgent.CustomAnimationTree.TrySetParam("stopped", true);
    }

    public override void StateExited()
    {
        base.StateExited();
        //CombatAgent.CustomAnimationTree.TrySetParam("stopped", false);
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
        
        // if (CombatAgent.readyToFire)
        // {
        //     OnStateChangeRequired(new ChangeStateEventArgs("AimingState"));
        //     return;
        // }
        
        if (CombatAgent.Velocity.LengthSquared() != 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("IsMovingState"));
            return;
        }
    }
}
