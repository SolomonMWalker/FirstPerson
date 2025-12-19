using FirstPerson.Configuration;
using FirstPerson.Helpers;
using Godot;

public partial class Fireball : CharacterBody3D
{
    private bool Initialized { get; set; }
    private bool VelocitySet { get; set; }
    private bool QueuedForFree { get; set; }
    private float Speed { get; set; } = 35;
    private double TimeToLive { get; set; } = 5;
    private Vector3 TargetGlobalPosition { get; set; }
    private Vector3 GlobalPositionSpawnPoint { get; set; }
    private Poll TimeToLivePoll { get; set; }
    private Node3D ProjectilesParent { get; set; }
    private PackedScene ExplosionPackedScene { get; set; }
    
    private const string ExplosionScenePath = "/explosion.tscn";

    public void Initialize(Vector3 targetGlobalPosition, Vector3 spawnPoint)
    {
        Initialized = true;
        TargetGlobalPosition = targetGlobalPosition;
        GlobalPositionSpawnPoint = spawnPoint;
    }

    public override void _Ready()
    {
        base._Ready();
        TimeToLivePoll = new Poll(TimeToLive);
        GlobalPosition = GlobalPositionSpawnPoint;
        ProjectilesParent = GetNode<Node3D>("/root/Test/ProjectilesParent");
        var explosionFullPath = Configuration.GetConfigValues().ProjectileDirectoryPath + ExplosionScenePath;
        ExplosionPackedScene = GD.Load<PackedScene>(explosionFullPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (QueuedForFree) return;
        if (TimeToLivePoll.IsPollPinged(delta))
        {
            QueuedForFree = true;
            return;
        }
        base._PhysicsProcess(delta);
        
        if (!VelocitySet)
        {
            VelocitySet = true;
            Velocity = GlobalPosition.DirectionTo(TargetGlobalPosition) * Speed;
        }
        var collision = MoveAndCollide(Velocity * (float)delta);
        var collider = collision?.GetCollider();
        if (collider is null) return;
        var explosion = ExplosionPackedScene.Instantiate<Explosion>();
        explosion.Initialize(GlobalPosition);
        ProjectilesParent.AddChild(explosion);
        QueuedForFree = true;
        QueueFree();
    }
}
