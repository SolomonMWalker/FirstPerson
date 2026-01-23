using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

[GlobalClass]
public partial class WeaponStateMachine : StateMachine
{
    [Export] public WeaponController WeaponController { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        SetUniqueNameInOwner(true);
    }
}
