using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

public partial class WeaponFiringState : WeaponAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        //WeaponController.FireWeapon();
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (WeaponController.GetCurrentWeaponAmmo() <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WeaponEmptyState"));
            return;
        }
        
        if (WeaponController.CurrentWeapon.IsAutomatic)
        {
            if (Input.IsActionPressed("Fire"))
            {
                if (WeaponController.CanFire())
                {
                    //WeaponController.FireWeapon();
                }
            }
            else
            {
                OnStateChangeRequired(new ChangeStateEventArgs("WeaponIdleState"));
                return;
            }
        }
        else
        {
            OnStateChangeRequired(new ChangeStateEventArgs("WeaponIdleState"));
            return;
        }
    }
}
