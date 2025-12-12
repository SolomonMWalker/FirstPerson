using Godot;
using System.Collections.Generic;

public partial class Explosion : Node3D
{
    public double LifetimeInSec { get; private set; } = 0.25;
    public int Damage { get; private set; } = 0;

    private List<CollisionObject3D> ObjectsHit { get; set; } = [];
    private ShapeCast3D ShapeCast3D { get; set; }
    private Vector3 InitialGlobalPosition { get; set; }
    private double TimeAlive { get; set; }
    
    public void Initialize(Vector3 globalPosition) => InitialGlobalPosition = globalPosition;

    public override void _Ready()
    {
        base._Ready();
        GlobalPosition = InitialGlobalPosition;
        ShapeCast3D = GetNode<ShapeCast3D>("ShapeCast3D");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (TimeAlive > LifetimeInSec)
        {
            QueueFree();
        }
        else
        {
            TimeAlive += delta;
        }

        if (!ShapeCast3D.IsColliding()) return;
        for (int index = 0; index < ShapeCast3D.GetCollisionCount(); index++)
        {
            var collided = ShapeCast3D.GetCollider(index);
            if (collided is CollisionObject3D colObj3D)
            {
                ShapeCast3D.AddException(colObj3D);
                ObjectsHit.Add(colObj3D);  
                if (colObj3D is HittableCharacterBody3D shootable)
                {
                    shootable.Hit(new HitParameters(Damage));
                }
            }
        }
    }
}