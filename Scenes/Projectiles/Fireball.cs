using FirstPerson.Configuration;
using Godot;

public partial class Fireball : CharacterBody3D
{
    private bool _initialized;
    private float _speed = 35;
    private Vector3 _targetGlobalPosition;
    private Vector3 _globalPositionSpawnPoint;

    private Node3D _projectilesParent;
    private PackedScene _explosionPackedScene;
    private const string _explosionScenePath = "/explosion.tscn";

    public void Initialize(Vector3 targetGlobalPosition, Vector3 spawnPoint)
    {
        _initialized = true;
        _targetGlobalPosition = targetGlobalPosition;
        _globalPositionSpawnPoint = spawnPoint;
    }

    public override void _Ready()
    {
        base._Ready();
        GlobalPosition = _globalPositionSpawnPoint;
        _projectilesParent = GetNode<Node3D>("/root/Test/ProjectilesParent");
        var explosionFullPath = Configuration.GetConfigValues().ProjectileDirectoryPath + _explosionScenePath;
        _explosionPackedScene = GD.Load<PackedScene>(explosionFullPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var directionToTarget = GlobalPosition.DirectionTo(_targetGlobalPosition);
        Velocity = directionToTarget * _speed;
        var collision = MoveAndCollide(Velocity * (float)delta);
        if (collision?.GetCollider() == null) return;
        var explosion = _explosionPackedScene.Instantiate<Explosion>();
        explosion.Initialize(GlobalPosition);
        _projectilesParent.AddChild(explosion);
        QueueFree();
    }
}
