using Godot;
using System;

public partial class Managers : Node
{
    public WeaponManager WeaponManager { get; set; }

    public override void _Ready()
    {
        base._Ready();
        CallDeferred("FindManagers");
    }

    public void FindManagers()
    {
        WeaponManager = (WeaponManager) GetTree().GetFirstNodeInGroup("WeaponManager");
    }
}
