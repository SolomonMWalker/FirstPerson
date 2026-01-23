using FirstPerson.Scenes.Player;
using Godot;

namespace FirstPerson.assets.weapons.scripts;

public partial class Projectile : Area3D
{
    [Export] public RayCast3D RayCast { get; set; }
    public bool IsSetup { get; set; }
    public Vector3 Velocity { get; set; }
    public float Damage { get; set; }

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
        RayCast.AddException(this);
        GetTree().CreateTimer(3.0).Timeout += QueueFree;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsSetup) return;
        base._PhysicsProcess(delta);
        float fDelta = (float) delta;
        var nextFrameLocation = GlobalPosition + Velocity * fDelta;
        RayCast.TargetPosition = RayCast.ToLocal(nextFrameLocation);
        RayCast.ForceRaycastUpdate();
        if (RayCast.IsColliding())
        {
            var hit = (Node3D) RayCast.GetCollider();
            var position = RayCast.GetCollisionPoint();
            GlobalPosition = position;
            OnBodyEntered(hit);
        }

        GlobalPosition = nextFrameLocation;
    }

    public void Setup(Vector3 velocity, float damage)
    {
        Velocity = velocity;
        Damage = damage;
        Monitoring = true;
        IsSetup = true;
    }

    public void OnBodyEntered(Node3D body)
    {
        GD.Print($"Projectile hit: {body.Name} at {GlobalPosition}");
        SpawnImpactMarker(GlobalPosition);
        QueueFree();
    }
    
    public void SpawnImpactMarker(Vector3 position)
    {
        var marker = new MeshInstance3D();
        var box = new BoxMesh();
        box.Size = new Vector3(0.1f, 0.1f, 0.1f);
        marker.Mesh = box;

        var material = new StandardMaterial3D();
        material.AlbedoColor = Colors.Red;
        marker.SetSurfaceOverrideMaterial(0, material);

        GetTree().CurrentScene.AddChild(marker);
        marker.GlobalPosition = position;

        GetTree().CreateTimer(2.0).Timeout += marker.QueueFree;
    }
}