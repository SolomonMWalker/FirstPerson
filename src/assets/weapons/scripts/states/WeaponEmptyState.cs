using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

public partial class WeaponEmptyState : WeaponAtomicState
{
    public override void StateEntered()
    {
        base.StateEntered();
        GD.Print("Weapon is empty!");
    }
}
