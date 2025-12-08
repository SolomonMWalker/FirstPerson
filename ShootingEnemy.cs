using Godot;
using System;

public partial class ShootingEnemy : Enemy
{    
    public double timeBetweenShots = 1.5;
    private double _timeSinceLastShot = 0;
    private PackedScene _fireballPackedScene;
    private Node3D _bulletSpawnPoint;
    
    public override void _Ready()
    {
        base._Ready();
        _target = GetNode<ShootableCharacterBody3D>("/root/Test/EnemyTarget");
        _fireballPackedScene = GD.Load<PackedScene>("res://fireball.tscn");
        _bulletSpawnPoint = GetNode<Node3D>("BulletSpawnPoint");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_currentBehaviorState == BehaviorState.AtCover)
        {
            if (_timeSinceLastShot > timeBetweenShots)
            {
                _timeSinceLastShot = 0;
                var fireBall = _fireballPackedScene.Instantiate<Fireball>();
                fireBall.Initialize(ToLocal(_target.GlobalPosition).Normalized(), _bulletSpawnPoint.Position);
                AddChild(fireBall);
            }
            else
            {
                _timeSinceLastShot += delta;
            }
        }
        else
        {
            _timeSinceLastShot = 0;
        }
    }
}
