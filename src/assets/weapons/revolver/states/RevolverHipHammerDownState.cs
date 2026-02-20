using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player;

public partial class RevolverHipHammerDownState : WeaponAtomicState
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
        if(WeaponController.CurrentWeaponModel.AnimationPlayer.IsPlaying()) return;

        if (Input.IsActionJustPressed("Fire") && WeaponController.CanFire())
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipFireState"));
            return;
        }

        if (Input.IsActionPressed("Aim") && !_playerController.Sprinting)
        {
            if(WeaponController.CurrentWeaponModel is not RevolverRig revolverRig) return;
            WeaponController.CurrentWeaponModel.StartAiming();
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
