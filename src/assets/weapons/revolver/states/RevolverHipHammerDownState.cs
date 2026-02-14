using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

public partial class RevolverHipHammerDownState : WeaponAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if(WeaponController.CurrentWeaponModel.AnimationPlayer.IsPlaying()) return;

        if (Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipFireState"));
            return;
        }

        if (Input.IsActionPressed("Aim"))
        {
            if(WeaponController.CurrentWeaponModel is not RevolverRig revolverRig) return;
            revolverRig.PlayHammerDownHipToAimAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimHammerDownState"));
            return;
        }

        if (Input.IsActionJustPressed("Reload") && WeaponController.CanReload())
        {
            WeaponController.CurrentWeaponModel.PlayHipToReloadAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverReloadState"));
            return;
        }
    }
}
