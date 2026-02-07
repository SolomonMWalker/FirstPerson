using Godot;

namespace FirstPerson.assets.weapons.scripts.revolver.states;

public partial class RevolverHipEmptyState : WeaponAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        GD.Print("Revolver is empty!");
    }
}