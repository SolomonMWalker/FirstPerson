using FirstPerson;
using FirstPerson.Configuration;
using Godot;

public partial class ShootingEnemy : Agent
{    
    public double TimeBetweenShots { get; protected set; } = 1.5;
    public double TimeToShoot { get; protected set; } = 0.3;
    public float MediumFollowDistance { get; protected set; } = 10f;
    
    protected double TimeSinceLastShot { get; set; } = 0;
    protected double TimeSinceShotForMovement { get; set; } = 0;
    protected bool IsShooting { get; set; }
    protected PackedScene FireballPackedScene { get; set; }
    protected Node3D BulletSpawnPoint { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        //Target = GetNode<HittableCharacterBody3D>("/root/Test/EnemyTarget");
        Target = GetNode<HittableCharacterBody3D>(Configuration.GetConfigValues().PlayerSceneTreePath);
        FireballPackedScene = GD.Load<PackedScene>($"{Configuration.GetConfigValues().ProjectileDirectoryPath}/fireball.tscn");
        BulletSpawnPoint = GetNode<Node3D>("BulletSpawnPoint");
        CurrentFollowDistance = MediumFollowDistance;
        AllowedGoals.Add(Goal.MoveToCover);
        CurrentGoal = Goal.MoveToCover;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        HandleShooting(delta);
    }
    
    protected override bool IsMotionFrozen()
    {
        return IsShooting || base.IsMotionFrozen();
    }
    
    protected override void CalculateNavigation(double delta)
    {
        switch (CurrentGoal)
        {
            case Goal.MoveToCover:
                MoveToCover(delta);
                break;
            case Goal.MoveToTargetMedium:
                MoveToTarget(delta);
                break;
        }
    }

    protected virtual void HandleShooting(double delta)
    {
        if (IsActivityFrozen())
        {
            if (IsStaggered) IsShooting = false;
            return;
        }
        
        //time between shots
        if (TimeSinceLastShot > TimeBetweenShots && TargetInLineOfSight)
        {
            TimeSinceLastShot = 0;
            TimeSinceShotForMovement = 0;
            IsShooting = true;
            LookAtTarget();
            var fireBall = FireballPackedScene.Instantiate<Fireball>();
            fireBall.Initialize(Target.GlobalPosition, BulletSpawnPoint.GlobalPosition);
            AddChild(fireBall);
        }
        else
        {
            TimeSinceLastShot += delta;
        }

        //how long to stop moving when shooting
        if (IsShooting)
        {
            if (TimeSinceShotForMovement > TimeToShoot)
            {
                IsShooting = false;
            }
            else
            {
                TimeSinceShotForMovement += delta;
            }
        }
    }

    protected override void HandleRotation()
    {
        if (IsShooting)
        {
            LookAtTarget();
        }
        else
        {
            base.HandleRotation();
        }
    }
}
