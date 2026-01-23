using Godot;
using System;

[GlobalClass]
public partial class Weapon : Resource
{
    [Export] public string WeaponName { get; set; } = "Pistol";
    [Export] public float Damage { get; set; } = 25.0f;
    [Export] public int MaxAmmo { get; set; } = 12;
    [Export] public float Range { get; set; } = 25f;
    [Export] public float ProjectileSpeed { get; set; } = 50f;
    [Export] public bool IsHitscan { get; set; } = true;
    [Export] public PackedScene WeaponModel { get; set; }
    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public Vector3 WeaponPosition { get; set; } = new (0.2f, -0.2f, -0.3f);
}
