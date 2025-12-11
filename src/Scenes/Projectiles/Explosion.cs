using Godot;
using System.Collections.Generic;

public partial class Explosion : Node3D
{
    public double lifetimeInSec = 0.25;
    public int damage = 0;

    private List<CollisionObject3D> _objectsHit = [];
    private ShapeCast3D _shapeCast3D;
    private Vector3 _initialGlobalPosition;
    private double _timeAlive;
    public void Initialize(Vector3 globalPosition) => _initialGlobalPosition = globalPosition;

    public override void _Ready()
    {
        base._Ready();
        GlobalPosition = _initialGlobalPosition;
        _shapeCast3D = GetNode<ShapeCast3D>("ShapeCast3D");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_timeAlive > lifetimeInSec)
        {
            QueueFree();
        }
        else
        {
            _timeAlive += delta;
        }

        if (!_shapeCast3D.IsColliding()) return;
        for (int index = 0; index < _shapeCast3D.GetCollisionCount(); index++)
        {
            var collided = _shapeCast3D.GetCollider(index);
            if (collided is CollisionObject3D colObj3D)
            {
                _shapeCast3D.AddException(colObj3D);
                _objectsHit.Add(colObj3D);  
                if (colObj3D is ShootableCharacterBody3D shootable)
                {
                    shootable.Shot(new ShotParameters(damage));
                }
            }
        }
    }
}