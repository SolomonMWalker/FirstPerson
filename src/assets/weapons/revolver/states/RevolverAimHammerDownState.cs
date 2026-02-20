using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverAimHammerDownState : WeaponAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if(WeaponController.CurrentWeaponModel.AnimationPlayer.IsPlaying()) return;
        
        if (Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimFireState"));
            return;
        }
        
        if (!Input.IsActionPressed("Aim") || !WeaponController.Aiming)
        {
            if(WeaponController.CurrentWeaponModel is not RevolverRig revolverRig) return;
            WeaponController.CurrentWeaponModel.StopAiming();
            revolverRig.PlayHammerDownAimToHipAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipHammerDownState"));
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