using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player;

[GlobalClass]
public partial class WeaponStateMachine : StateMachine
{
    public WeaponController WeaponController { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        WeaponController = (WeaponController)GetTree().GetFirstNodeInGroup("weaponController");
    }
}
