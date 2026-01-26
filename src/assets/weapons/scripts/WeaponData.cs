using Godot;
using System;

[GlobalClass]
public partial class WeaponData : Resource
{
    [Export] public Weapon Weapon { get; set; }
    [Export] public bool Unlocked { get; set; }
    [Export] public int Ammo { get; set; }
}
