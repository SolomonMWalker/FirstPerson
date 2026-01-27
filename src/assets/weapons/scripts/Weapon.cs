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
    [Export] public int AccuracyPercent
    {
        get => _accuracyPercent;
        set => _accuracyPercent = Mathf.Clamp(value, 0, 100);
    } private int _accuracyPercent = 100;
    [Export] public int AccuracyPenaltyAtMaxMovementSpeed
    {
        get => _accuracyPenaltyAtMaxMovementSpeed;
        set => _accuracyPenaltyAtMaxMovementSpeed = Mathf.Clamp(value, 0, 100);
    } private int _accuracyPenaltyAtMaxMovementSpeed = 100;
    [Export] public float ProjectileSpeed { get; set; } = 50f;
    [Export] public bool IsHitscan { get; set; } = true;
    [Export] public PackedScene WeaponModel { get; set; }
    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public int PelletCount = 1;
    [Export] public float SpreadAngle = 0;
    [Export] public Vector3 WeaponPosition { get; set; } = new (0.2f, -0.2f, -0.3f);
    [Export] public Vector2 SwayMin { get; set; } = new (-20f, -20f);
    [Export] public Vector2 SwayMax { get; set; } = new (20f, 20f);
    [Export] public float SwaySpeedPosition
    {
        get => _swaySpeedPosition;
        set => _swaySpeedPosition = Mathf.Clamp(value, 0f, 0.2f);
    } private float _swaySpeedPosition = 0.07f;
    [Export] public float SwaySpeedRotation
    {
        get => _swaySpeedRotation;
        set => _swaySpeedRotation = Mathf.Clamp(value, 0, 0.2f);
    } private float _swaySpeedRotation = 0.1f;
    [Export] public float SwayAmountPosition
    {
        get => _swayAmountPosition;
        set => _swayAmountPosition = Mathf.Clamp(value, 0, 0.25f);
    } private float _swayAmountPosition = 0.1f;
    [Export] public float SwayAmountRotationInDeg
    {
        get => _swayAmountRotationInDeg;
        set => _swayAmountRotationInDeg = Mathf.Clamp(value, 0, 50f);
    } private float _swayAmountRotationInDeg = 30f;
    
}
