using System.Collections.Generic;
using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverReloadState : WeaponAtomicState
{
    public bool interrupted;
    public WeaponManager WeaponManager { get; private set; }
    public Timer Timer {get; private set;}
    public override void _Ready()
    {
        base._Ready();
        WeaponManager = (WeaponManager) GetTree().GetFirstNodeInGroup("WeaponManager");
        Timer = new Timer();
        Timer.Autostart = false;
        Timer.OneShot = true;
        Timer.WaitTime = 0.2f;
        AddChild(Timer);
    }

    public override void StateEntered()
    {
        base.StateEntered();
        var bulletsToReload = WeaponController.CurrentWeapon.MaxAmmo - WeaponManager.Weapons[WeaponManager.CurrentSlot].Ammo;
        GD.Print($"Weapon max ammo is {WeaponController.CurrentWeapon.MaxAmmo}, " +
                 $"weapon has {WeaponManager.Weapons[WeaponManager.CurrentSlot].Ammo} ammo, " +
                 $"reloading {bulletsToReload} ammo");
        WeaponController.CurrentWeaponModel.PlayReloadAnimation(bulletsToReload);
        Timer.Start();
    }

    public override void StateExited()
    {
        base.StateExited();
        interrupted = false;
    }


    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if(!Timer.IsStopped()) return;
        if (!WeaponController.CurrentWeaponModel.IsAnimationPlaying())
        {
            WeaponController.CurrentWeaponModel.PlayHipIdleAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipIdleState"));
            return;
        }

        if(Input.IsActionJustPressed("Fire"))
        {
            interrupted = true;
            if (WeaponController.CurrentWeaponModel is not RevolverRig revolverRig) return;
            if (revolverRig.InterruptibleReloadAnimations.Contains(revolverRig.AnimationPlayer.CurrentAnimation))
            {
                revolverRig.AnimationPlayer.Play(revolverRig.ReloadInterruptAnimationName);
                revolverRig.AnimationPlayer.Queue(revolverRig.ReloadCloseCylinderStartAnimationName);
                revolverRig.AnimationPlayer.Queue(revolverRig.ReloadCloseCylinderEndAnimationName);
            }
            else
            {
                var indexOfCurrentAnim = revolverRig.PostCylinderTurnInterruptReloadPath.IndexOf(
                    revolverRig.AnimationPlayer.CurrentAnimation);
                if (indexOfCurrentAnim < 0) return;
                for (int i = indexOfCurrentAnim; i < revolverRig.PostCylinderTurnInterruptReloadPath.Count; i++)
                {
                    if(i == indexOfCurrentAnim) revolverRig.AnimationPlayer.Play(
                        revolverRig.PostCylinderTurnInterruptReloadPath[i]);
                    else revolverRig.AnimationPlayer.Queue(
                        revolverRig.PostCylinderTurnInterruptReloadPath[i]);
                }
            }
        }
        
    }
}