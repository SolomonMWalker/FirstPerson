using Godot;
using System;

[GlobalClass]
public partial class Weapon : Resource
{
    [Export] public string WeaponName { get; set; } = "Pistol";
    [Export] public float Damage { get; set; } = 25.0f;
    [Export] public int MaxAmmo { get; set; } = 12;
    [Export] public float FireRatePerSecond { get; set; } = 2.0f;
    [Export] public bool IsAutomatic { get; set; } = false;
    [Export] public float Range { get; set; } = 25f;
    [Export] public int Accuracy
        {
            get => _accuracy;
            set => _accuracy = Mathf.Clamp(value, 0, 100);
        }
    private int _accuracy = 100;
    [Export] public float ProjectileSpeed { get; set; } = 50f;
    [Export] public bool IsHitscan { get; set; } = true;
    [Export] public PackedScene WeaponModel { get; set; }
    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public int PelletCount = 1;
    [Export] public float SpreadAngle = 0;
    [Export] public Vector3 WeaponPosition { get; set; } = new (0.2f, -0.2f, -0.3f);
}
