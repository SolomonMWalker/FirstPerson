using Godot;
using System;

public partial class WeaponController : Node
{
    [Export] public Weapon CurrentWeapon { get; set; }
    [Export] public Node3D WeaponModelParent { get; set; }
    [Export] public WeaponStateMachine WeaponStateMachine { get; set; }

    public Node3D currentWeaponModel;
    public int currentAmmo;

    public override void _Ready()
    {
        base._Ready();
        if (CurrentWeapon is not null)
        {
            SpawnWeaponModel();
            currentAmmo = CurrentWeapon.MaxAmmo;
        }
    }

    public void SpawnWeaponModel()
    {
        currentWeaponModel?.QueueFree();
        if (CurrentWeapon.WeaponModel is not null)
        {
            currentWeaponModel = CurrentWeapon.WeaponModel.Instantiate<Node3D>();
            WeaponModelParent.AddChild(currentWeaponModel);
            currentWeaponModel.Position = CurrentWeapon.WeaponPosition;
        }
    }

    public bool CanFire() => currentAmmo > 0;

    public void FireWeapon()
    {
        if (CanFire())
        {
            currentAmmo -= 1;
            GD.Print($"Fired! ammo at {currentAmmo}");
        }
    }
}
