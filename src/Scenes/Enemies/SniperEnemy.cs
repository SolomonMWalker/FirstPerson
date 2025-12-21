using FirstPerson.Helpers;
using Godot;

public partial class SniperEnemy : ShootingEnemy
{
    [Export] public float TimeToCharge { get; private set; } = 1;
    public Poll TimeToChargePoll { get; private set; }
    public MeshInstance3D ChargeAura { get; private set; }
    public bool IsCharging { get; private set; }
    public bool ReadyToCharge { get; private set; }
    public Vector3 WhereTargetWillBe { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        TimeToChargePoll = new Poll(TimeToCharge, 0);
        ChargeAura = GetNode<MeshInstance3D>("Hand/Gun/ChargeAura");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!ShouldSkipShooting()) HandleShooting(delta);
        CalculateStayStillForShooting(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleCharging(delta);
    }

    protected virtual bool ShouldSkipCharging()
    {
        if (IsActivityFrozen()) return true;
        if (!IsStaggered) return false;
        ChargeAura.Visible = false;
        IsCharging = false;
        IsStayingStillForShot = false;
        TimeSinceLastShotPoll.ResetPoll();
        return true;
    }
    
    protected override bool ShouldSkipShooting()
    {
        if (IsActivityFrozen()) return true;
        if (!IsStaggered) return false;
        IsStayingStillForShot = false;
        TimeSinceLastShotPoll.ResetPoll();
        return true;
    }

    protected override bool IsMotionFrozen()
    {
        return IsCharging || base.IsMotionFrozen();
    }

    protected override bool IsRotationFrozen()
    {
        return IsCharging || base.IsRotationFrozen();
    }

    public void HandleCharging(double delta)
    {
        if (IsCharging)
        {
            if (TimeToChargePoll.IsPollPinged(delta))
            {
                ReadyToShoot = true;
            }
            return;
        }
        if (ReadyToCharge)
        {
            ReadyToCharge = false;
            CalculateIfTargetInLineOfSight();
            if (!TargetInLineOfSight)
            {
                TimeSinceLastShotPoll.ResetPoll();
                return;
            }
            IsCharging = true;
            TimeToChargePoll.ResetPoll();
            CalculateWhereTargetWillBe();
            LookAtTargetForCharge();
            ChargeAura.Visible = true;
            return;
        }
        if (TimeSinceLastShotPoll.IsPollPinged(delta))
        {
            ReadyToCharge = true;
        }
    }

    public void CalculateWhereTargetWillBe()
    {
        WhereTargetWillBe = new Vector3(
            Target.GlobalPosition.X + Target.Velocity.X * TimeToCharge,
            Target.GlobalPosition.Y,
            Target.GlobalPosition.Z + Target.Velocity.Z * TimeToCharge);
    }

    protected override void HandleShooting(double delta)
    {
        //time between shots
        if (ReadyToShoot)
        {
            TimeToStayStillForShotPoll.ResetPoll();
            TimeSinceLastShotPoll.ResetPoll();
            ChargeAura.Visible = false;
            IsStayingStillForShot = true;
            ReadyToShoot = false;
            IsCharging = false;
            var fireBall = FireballPackedScene.Instantiate<Fireball>();
            fireBall.Initialize(WhereTargetWillBe, BulletSpawnPoint.GlobalPosition, ProjectileSpeed);
            AddChild(fireBall);
        }
    }

    protected virtual void LookAtTargetForCharge()
    {
        var target = WhereTargetWillBe;
        var rotVector = HelperMethods.GetAxisRotationsToTarget(this, target);
        Rotation = new Vector3(Rotation.X, rotVector.Y, Rotation.Z);
        Hand.LookAt(target);
    }
}
