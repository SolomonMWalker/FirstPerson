using Godot;

public partial class Fireball : CharacterBody3D
{
    private bool _initialized;
    private float _speed = 10;
    private Vector3 _targetGlobalPosition;
    private Vector3 _globalPositionSpawnPoint;


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
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var directionToTarget = GlobalPosition.DirectionTo(_targetGlobalPosition);
        Velocity = directionToTarget * _speed;
        var collision = MoveAndCollide(Velocity * (float)delta);
        if (collision == null) return;
        if (collision.GetCollider() is ShootableCharacterBody3D shootable)
        {
            shootable.Shot(new ShotParameters(10));
        }
        QueueFree();
    }
}
