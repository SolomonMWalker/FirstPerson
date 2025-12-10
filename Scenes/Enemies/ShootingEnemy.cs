using FirstPerson.Configuration;
using Godot;

public partial class ShootingEnemy : Enemy
{    
    public double timeBetweenShots = 1.5;
    public double timeToShoot = 0.3;
    
    private double _timeSinceLastShot = 0;
    private double _timeSinceShotForMovement = 0;
    private bool _isShooting;
    private PackedScene _fireballPackedScene;
    private Node3D _bulletSpawnPoint;   
    
    
    public override void _Ready()
    {
        base._Ready();
        _target = GetNode<ShootableCharacterBody3D>("/root/Test/EnemyTarget");
        _fireballPackedScene = GD.Load<PackedScene>($"{Configuration.GetConfigValues().ProjectileDirectoryPath}/fireball.tscn");
        _bulletSpawnPoint = GetNode<Node3D>("BulletSpawnPoint");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        HandleShooting(delta);
    }

    public void HandleShooting(double delta)
    {
        //time between shots
        if (_timeSinceLastShot > timeBetweenShots)
        {
            _timeSinceLastShot = 0;
            _timeSinceShotForMovement = 0;
            _isShooting = true;
            LookAtTarget();
            var fireBall = _fireballPackedScene.Instantiate<Fireball>();
            fireBall.Initialize(_target.GlobalPosition, _bulletSpawnPoint.GlobalPosition);
            AddChild(fireBall);
        }
        else
        {
            _timeSinceLastShot += delta;
        }

        //how long to stop moving when shooting
        if (_isShooting)
        {
            if (_timeSinceShotForMovement > timeToShoot)
            {
                _isShooting = false;
            }
            else
            {
                _timeSinceShotForMovement += delta;
            }
        }
    }

    protected override void HandleNavigation()
    {
        if (_isShooting)
        {
            Velocity = Vector3.Zero;
            return;
        }
        base.HandleNavigation();
    }

    protected override void HandleRotation()
    {
        if (_isShooting)
        {
            LookAtTarget();
        }
        base.HandleRotation();
    }
}
