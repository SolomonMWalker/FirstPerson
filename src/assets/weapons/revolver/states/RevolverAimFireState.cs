using FirstPerson.CustomTypes.StateMachine;
using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverAimFireState : WeaponAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        WeaponController.FireWeapon(isAiming: true);
    }

    public override void StatePhysicsProcessing(double delta)
    {
        base.StateProcessing(delta);
        OnStateChangeRequired(new ChangeStateEventArgs("RevolverAimIdleState"));
        return;
    }
}