using FirstPerson;
using FirstPerson.Configuration;
using FirstPerson.Helpers;
using Godot;

public partial class ShootingEnemy : Agent
{    
    public double TimeBetweenShots { get; protected set; } = 1.5;
    public double TimeToShoot { get; protected set; } = 0.3;
    public double TimeBetweenAccuracyChecks { get; protected set; } = 0.2;
    public float MediumFollowDistance { get; protected set; } = 10f;
    
    protected Poll TimeSinceLastShotPoll { get; set; }
    protected Poll TimeSinceLastShotForMovementPoll { get; set; }
    protected Poll TimeSinceAccuracyCheckPoll { get; set; }
    protected AccuracyController AccuracyController { get; set; }
    protected PackedScene FireballPackedScene { get; set; }
    protected Node3D BulletSpawnPoint { get; set; }
    protected bool IsShooting { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        UseMoveToTargetFuzziness = true;
        //Target = GetNode<HittableCharacterBody3D>("/root/Test/EnemyTarget");
        Target = GetNode<FirstPerson.CustomTypes.HittableCharacterBody3D>(Configuration.GetConfigValues().PlayerSceneTreePath);
        TimeSinceLastShotPoll = new Poll(TimeBetweenShots + Fuzzer.Fuzz(0f, 0.3f, false));
        TimeSinceLastShotForMovementPoll = new Poll(TimeToShoot + Fuzzer.Fuzz(0f, 0.3f, false));
        TimeSinceAccuracyCheckPoll = new Poll(TimeBetweenAccuracyChecks + Fuzzer.Fuzz(0f, 0.05f, false));
        AccuracyController = new AccuracyController();
        FireballPackedScene = GD.Load<PackedScene>($"{Configuration.GetConfigValues().ProjectileDirectoryPath}/fireball.tscn");
        BulletSpawnPoint = GetNode<Node3D>("BulletSpawnPoint");
        CurrentFollowDistance = MediumFollowDistance;
        AllowedGoals.Add(Goal.MoveToCover);
        CurrentGoal = Goal.MoveToCover;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(!ShouldSkipShooting()) HandleShooting(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (TimeSinceAccuracyCheckPoll.IsPollPinged(delta))
        {
            AccuracyController.CheckTargetForAccuracy(TimeBetweenAccuracyChecks, Target);
        }
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

    protected virtual bool ShouldSkipShooting()
    {
        if (!IsActivityFrozen()) return false;
        if (!IsStaggered) return true;
        IsShooting = false;
        TimeSinceLastShotPoll.ResetPoll();
        return true;
    }

    protected virtual void HandleShooting(double delta)
    {
        //time between shots
        if(TimeSinceLastShotPoll.IsPollPinged(delta))
        {
            TimeSinceLastShotForMovementPoll.ResetPoll();
            IsShooting = true;
            LookAtTarget();
            var fireBall = FireballPackedScene.Instantiate<Fireball>();
            var accuracyAppliedTargetPosition = AccuracyController.ApplyAccuracyToTargetPosition(Target.GlobalPosition);
            fireBall.Initialize(accuracyAppliedTargetPosition, BulletSpawnPoint.GlobalPosition);
            AddChild(fireBall);
        }

        //how long to stop moving when shooting
        if (!IsShooting) return;
        //if (TimeSinceShotForMovement > TimeToShoot)
        if(TimeSinceLastShotForMovementPoll.IsPollPinged(delta))
        {
            IsShooting = false;
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
