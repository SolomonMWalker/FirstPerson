using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverAimIdleState : WeaponAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        
        if (Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        {
            if(WeaponController.CurrentWeaponModel is not RevolverRig revolverRig) return;
            revolverRig.PlayAimHammerDownAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimHammerDownState"));
            return;
        }
        
        if (!Input.IsActionPressed("Aim") || !WeaponController.Aiming)
        {
            //play to hip animation because hip animation doesn't know how its being reached
            WeaponController.CurrentWeaponModel.StopAiming();
            WeaponController.CurrentWeaponModel.PlayAimToHipAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipIdleState"));
            return;
        }

        if (WeaponController.GetCurrentWeaponAmmo() <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimEmptyState"));
            return;
        }

        if (Input.IsActionJustPressed("Reload") && WeaponController.CanReload())
        {
            WeaponController.CurrentWeaponModel.StopAiming();
            WeaponController.CurrentWeaponModel.PlayAimToReloadAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverReloadState"));
            return;
        }
    }
}