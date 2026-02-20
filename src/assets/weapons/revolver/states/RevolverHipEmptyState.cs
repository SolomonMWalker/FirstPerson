using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverHipEmptyState : WeaponAtomicState
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
        if (Input.IsActionPressed("Aim") && !_playerController.Sprinting)
        {
            //play to hip animation because hip animation doesn't know how its being reached
            WeaponController.CurrentWeaponModel.StartAiming();
            WeaponController.CurrentWeaponModel.PlayHipToAimAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimIdleState"));
            return;
        }

        if (Input.IsActionJustPressed("Reload") || Input.IsActionJustPressed("Fire"))
        {
            WeaponController.CurrentWeaponModel.PlayHipToReloadAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverReloadState"));
            return;
        }
    }
}