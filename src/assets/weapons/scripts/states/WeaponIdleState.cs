using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

public partial class WeaponIdleState : WeaponAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WeaponFiringState"));
            return;
        }
        
        if (WeaponController.currentAmmo <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WeaponEmptyState"));
        }
    }
}
