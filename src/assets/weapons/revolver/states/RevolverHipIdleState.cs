using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player;

public partial class RevolverHipIdleState : WeaponAtomicState
{
    private PlayerController _playerController;
    
    public override void _Ready()
    {
        base._Ready();
        _playerController = (PlayerController) GetTree().GetFirstNodeInGroup("player");
    }
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        {
            if(WeaponController.CurrentWeaponModel is not RevolverRig revolverRig) return;
            revolverRig.PlayHipHammerDownAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipHammerDownState"));
            return;
        }

        if (Input.IsActionPressed("Aim") && !_playerController.Sprinting)
        {
            WeaponController.CurrentWeaponModel.StartAiming();
            WeaponController.CurrentWeaponModel.PlayHipToAimAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimIdleState"));
            return;
        }
        
        if (WeaponController.GetCurrentWeaponAmmo() <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipEmptyState"));
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
