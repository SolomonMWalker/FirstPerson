using Godot;
using System;

[GlobalClass]
public partial class Weapon : Resource
{
    [Export] public string WeaponName { get; set; } = "Pistol";
    [Export] public float Damage { get; set; } = 25.0f;
    [Export] public int MaxAmmo { get; set; } = 12;
    [Export] public PackedScene WeaponModel { get; set; }
    [Export] public Vector3 WeaponPosition { get; set; } = new (0.2f, -0.2f, -0.3f);
}
