using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

[GlobalClass]
public partial class WeaponAtomicState : AtomicState
{
    public WeaponController WeaponController { get; set; }

    public override void _Ready()
    {
        base._Ready();
        WeaponController = GetNode<WeaponStateMachine>("%WeaponStateMachine").WeaponController;
    }
}
