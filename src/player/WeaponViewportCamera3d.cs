using Godot;
using System;

public partial class WeaponViewportCamera3d : Camera3D
{
    [Export] public CameraController CameraController { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);
        GlobalTransform = CameraController.GlobalTransform;
    }
}
