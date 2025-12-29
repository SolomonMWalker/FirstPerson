using FirstPerson;
using FirstPerson.Configuration;
using FirstPerson.CustomTypes;
using FirstPerson.Helpers;
using Godot;

public partial class ShootingEnemy : Agent
{    
    [Export] public double TimeBetweenShots { get; protected set; } = 1.5;
    [Export] public double TimeToShoot { get; protected set; } = 0.3;
    [Export] public double TimeBetweenAccuracyChecks { get; protected set; } = 0.2;
    [Export] public float ProjectileSpeed { get; protected set; } = 10f;
    [Export] public bool UseAccuracyFuzziness { get; protected set; }
    [Export] public float MaxHandVerticalAngleInDeg { get; protected set; } = 60;
    
    protected Poll TimeSinceLastShotPoll { get; set; }
    protected Poll TimeToStayStillForShotPoll { get; set; }
    protected Poll TimeSinceAccuracyCheckPoll { get; set; }
    protected AccuracyController AccuracyController { get; set; }
    protected PackedScene FireballPackedScene { get; set; }
    protected Node3D Hand { get; set; }
    protected Node3D BulletSpawnPoint { get; set; }
    protected bool IsStayingStillForShot { get; set; }
    protected bool ReadyToShoot { get; set; }
    protected float MaxHandVerticalAngleInRad { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        MaxHandVerticalAngleInRad = Mathf.DegToRad(MaxHandVerticalAngleInDeg);
        CombatTarget = GetNode<HittableCharacterBody3D>("/root/Test/EnemyTarget");
        //Target = GetNode<HittableCharacterBody3D>(Configuration.GetConfigValues().PlayerSceneTreePath);
        TimeSinceLastShotPoll = new Poll(TimeBetweenShots + Fuzzer.Fuzz(0f, 0.3f, false));
        TimeToStayStillForShotPoll = new Poll(TimeToShoot + Fuzzer.Fuzz(0f, 0.3f, false));
        TimeSinceAccuracyCheckPoll = new Poll(TimeBetweenAccuracyChecks + Fuzzer.Fuzz(0f, 0.05f, false));
        AccuracyController = new AccuracyController();
        FireballPackedScene = GD.Load<PackedScene>($"{Configuration.GetConfigValues().ProjectileDirectoryPath}/fireball.tscn");
        BulletSpawnPoint = GetNode<Node3D>("Hand/Gun/BulletSpawnPoint");
        Hand = GetNode<Node3D>("Hand");
        AllowedGoals.AddRange([Goal.MoveToCover, Goal.MoveToSpot, Goal.MoveToTarget]);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(!ShouldSkipShooting()) HandleShooting(delta);
        CalculateStayStillForShooting(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (TimeSinceAccuracyCheckPoll.IsPollPinged(delta))
        {
            AccuracyController.CheckTargetForAccuracy(TimeBetweenAccuracyChecks, CombatTarget);
        }
    }

    protected virtual void CalculateStayStillForShooting(double delta)
    {
        if (IsStayingStillForShot && TimeToStayStillForShotPoll.IsPollPinged(delta))
        {
            IsStayingStillForShot = false;
        }
    }
    
    protected override void LookAtTarget()
    {
        var rotVector = HelperMethods.GetAxisRotationsToTarget(this, CombatTarget.GlobalPosition);
        Rotation = new Vector3(Rotation.X, rotVector.Y, Rotation.Z);
        Hand.LookAt(CombatTarget.GlobalPosition);
        var clampedXRotation = Mathf.Clamp(Hand.Rotation.X, -MaxHandVerticalAngleInRad, MaxHandVerticalAngleInRad);
        Hand.Rotation = new Vector3(clampedXRotation, Hand.Rotation.Y, Hand.Rotation.Z);
    }

    protected override bool IsMotionFrozen()
    {
        return IsStayingStillForShot || base.IsMotionFrozen();
    }
    
    protected override bool IsRotationFrozen()
    {
        return IsStayingStillForShot || base.IsRotationFrozen();
    }
    
    protected override void CalculateNavigation(double delta)
    {
        switch (CurrentGoal)
        {
            case Goal.MoveToCover:
                MoveToCover(delta);
                break;
            case Goal.MoveToTarget:
                MoveToCombatTarget(delta);
                break;
            case Goal.MoveToSpot:
                MoveToMovementTarget(delta);
                break;
        }
    }

    protected virtual bool ShouldSkipShooting()
    {
        if (IsActivityFrozen()) return true;
        if (!IsStaggered) return false;
        ReadyToShoot = false;
        IsStayingStillForShot = false;
        TimeSinceLastShotPoll.ResetPoll();
        return true;
    }

    protected virtual void HandleShooting(double delta)
    {
        if (TimeSinceLastShotPoll.IsPollPinged(delta))
        {
            ReadyToShoot = true;
        }
        //time between shots
        if(ReadyToShoot)
        {
            ReadyToShoot = false;
            if (!TargetInLineOfSight)
            {
                TimeSinceLastShotPoll.ResetPoll();
                return;
            }
            TimeToStayStillForShotPoll.ResetPoll();
            IsStayingStillForShot = true;
            var fireBall = FireballPackedScene.Instantiate<Fireball>();
            var targetPosition = UseAccuracyFuzziness
                ? AccuracyController.ApplyAccuracyToTargetPosition(CombatTarget.GlobalPosition)
                : CombatTarget.GlobalPosition;
            var accuracyAppliedTargetPosition = targetPosition;
            fireBall.Initialize(accuracyAppliedTargetPosition, BulletSpawnPoint.GlobalPosition, ProjectileSpeed);
            AddChild(fireBall);
        }
    }
}
