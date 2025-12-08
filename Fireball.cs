using Godot;
using System;
using Godot.NativeInterop;

public partial class Fireball : CharacterBody3D
{
    private float _speed = 5;
    private Vector3 _direction;


    public void Initialize(Vector3 direction, Vector3 spawnPoint)
    {
        _direction = direction;
        Position = spawnPoint;
    }

    public override void _Ready()
    {
        base._Ready();
        LookAt(_direction);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Velocity = Vector3.Forward * _speed;
        var collision = MoveAndCollide(Velocity * (float)delta);
        if (collision == null) return;
        if (collision.GetCollider() is ShootableCharacterBody3D shootable)
        {
            shootable.Shot(new ShotParameters(10));
        }
        QueueFree();
    }
}
