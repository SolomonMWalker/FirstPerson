using FirstPerson.Configuration;
using Godot;

public partial class Fireball : CharacterBody3D
{
    private bool Initialized { get; set; }
    private bool VelocitySet { get; set; }
    private float Speed { get; set; } = 35;
    private Vector3 TargetGlobalPosition { get; set; }
    private Vector3 GlobalPositionSpawnPoint { get; set; }

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
        GlobalPosition = GlobalPositionSpawnPoint;
        ProjectilesParent = GetNode<Node3D>("/root/Test/ProjectilesParent");
        var explosionFullPath = Configuration.GetConfigValues().ProjectileDirectoryPath + ExplosionScenePath;
        ExplosionPackedScene = GD.Load<PackedScene>(explosionFullPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!VelocitySet)
        {
            VelocitySet = true;
            Velocity = GlobalPosition.DirectionTo(TargetGlobalPosition) * Speed;
        }
        var collision = MoveAndCollide(Velocity * (float)delta);
        if (collision?.GetCollider() == null) return;
        var explosion = ExplosionPackedScene.Instantiate<Explosion>();
        explosion.Initialize(GlobalPosition);
        ProjectilesParent.AddChild(explosion);
        Free();
    }
}
