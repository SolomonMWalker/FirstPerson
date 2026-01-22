using Godot;
using System;

public partial class WeaponController : Node
{
    [Export] public Weapon CurrentWeapon { get; set; }
    [Export] public Node3D WeaponModelParent { get; set; }

    public Node3D currentWeaponModel;

    public override void _Ready()
    {
        base._Ready();
        if (CurrentWeapon is not null)
        {
            
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
}
