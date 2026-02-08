using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

public partial class RevolverHipIdleState : WeaponAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipFireState"));
            return;
        }

        if (Input.IsActionPressed("Aim"))
        {
            WeaponController.CurrentWeaponModel.PlayHipToAimAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimIdleState"));
            return;
        }
        
        if (WeaponController.GetCurrentWeaponAmmo() <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipEmptyState"));
            return;
        }
    }
}
