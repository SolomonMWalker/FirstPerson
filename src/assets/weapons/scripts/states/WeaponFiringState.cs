using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

public partial class WeaponFiringState : WeaponAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        WeaponController.FireWeapon();
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (WeaponController.CurrentAmmo <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WeaponEmptyState"));
            return;
        }
        
        OnStateChangeRequired(new ChangeStateEventArgs("WeaponIdleState"));
    }
}
