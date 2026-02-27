using FirstPerson.Helpers;
using Godot;

public partial class Fireball : CharacterBody3D
{
    private static string PathToFireballScene = $"/fireball.tscn";
    private static PackedScene FireballPackedScene;
    
    private bool VelocitySet { get; set; }
    private bool QueuedForFree { get; set; }
    private float Speed { get; set; } = 35;
    private double TimeToLive { get; set; } = 5;
    private Vector3 TargetGlobalPosition { get; set; }
    private Vector3 GlobalPositionSpawnPoint { get; set; }
    //private Poll TimeToLivePoll { get; set; }
    private Node3D ProjectilesParent { get; set; }
    private PackedScene ExplosionPackedScene { get; set; }
    
    public static void Initialize(Node3D parent, Vector3 targetGlobalPosition, Vector3 spawnPoint, float? speed = null)
    {
        FireballPackedScene ??= GD.Load<PackedScene>(PathToFireballScene);
        var fireball = FireballPackedScene.Instantiate<Fireball>();
        fireball.InitializeValues(targetGlobalPosition, spawnPoint, speed);
        parent.AddChild(fireball);
    }

    private void InitializeValues(Vector3 targetGlobalPosition, Vector3 spawnPoint, float? speed = null)
    {
        TargetGlobalPosition = targetGlobalPosition;
        GlobalPositionSpawnPoint = spawnPoint;
        Speed = speed ?? Speed;
    }

    public override void _Ready()
    {
        base._Ready();
        //TimeToLivePoll = new Poll(TimeToLive, 0);
        GlobalPosition = GlobalPositionSpawnPoint;
        ProjectilesParent = GetNode<Node3D>("/root/Test/ProjectilesParent");
    }

    public override void _PhysicsProcess(double delta)
    {
        // if (QueuedForFree || TimeToLivePoll.IsPollPinged(delta))
        // {
        //     QueueFree();
        //     return;
        // }
        base._PhysicsProcess(delta);
        
        if (!VelocitySet)
        {
            VelocitySet = true;
            Velocity = GlobalPosition.DirectionTo(TargetGlobalPosition) * Speed;
        }
        var collision = MoveAndCollide(Velocity * (float)delta);
        var collider = collision?.GetCollider();
        if (collider is null) return;
        Explosion.Initialize(ProjectilesParent, GlobalPosition);
        QueuedForFree = true;
        QueueFree();
    }
}
