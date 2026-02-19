using Godot;
using System;

public partial class RevolverBulletCasingPhysics : RigidBody3D
{
    public override void _Ready()
    {
        base._Ready();
        GetTree().CreateTimer(3.0).Timeout += QueueFree;
    }
}
