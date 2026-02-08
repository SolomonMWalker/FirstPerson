using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverHipFireState : WeaponAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        WeaponController.FireWeapon(isAiming: false);
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (WeaponController.GetCurrentWeaponAmmo() <= 0)
        {
            OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipEmptyState"));
            return;
        }
        
        OnStateChangeRequired(new ChangeStateEventArgs("RevolverHipIdleState"));
        return;
    }
}