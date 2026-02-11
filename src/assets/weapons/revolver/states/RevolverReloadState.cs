using System.Collections.Generic;
using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverReloadState : WeaponAtomicState
{
    public WeaponManager WeaponManager { get; private set; }
    public override void _Ready()
    {
        base._Ready();
        WeaponManager = (WeaponManager) GetTree().GetFirstNodeInGroup("WeaponManager");
    }

    public override void StateEntered()
    {
        base.StateEntered();
        var bulletsToReload = WeaponController.CurrentWeapon.MaxAmmo - WeaponManager.Weapons[WeaponManager.CurrentSlot].Ammo;
        GD.Print($"Weapon max ammo is {WeaponController.CurrentWeapon.MaxAmmo}, " +
                 $"weapon has {WeaponManager.Weapons[WeaponManager.CurrentSlot].Ammo} ammo, " +
                 $"reloading {bulletsToReload} ammo");
        WeaponController.CurrentWeaponModel.PlayReloadAnimation(bulletsToReload);
    }

    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (!WeaponController.CurrentWeaponModel.IsAnimationPlaying())
        {
            WeaponController.CurrentWeaponModel.PlayHipIdleAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipIdleState"));
            return;
        }
    }
}