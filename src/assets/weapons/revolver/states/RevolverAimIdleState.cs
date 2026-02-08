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
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimFireState"));
            return;
        }
        
        if (!Input.IsActionPressed("Aim"))
        {
            //play to hip animation because hip animation doesn't know how its being reached
            WeaponController.CurrentWeaponModel.PlayAimToHipAnimation();
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipIdleState"));
        }
        
        if (WeaponController.GetCurrentWeaponAmmo() <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimEmptyState"));
            return;
        }
    }
}