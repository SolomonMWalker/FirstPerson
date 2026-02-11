using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverHipEmptyState : WeaponAtomicState
{
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (Input.IsActionPressed("Aim"))
        {
            //play to hip animation because hip animation doesn't know how its being reached
            WeaponController.CurrentWeaponModel.PlayHipToAimAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimIdleState"));
            return;
        }

        if (Input.IsActionJustPressed("Fire"))
        {
            WeaponController.CurrentWeaponModel.PlayHipToReloadAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverReloadState"));
            return;
        }
    }
}