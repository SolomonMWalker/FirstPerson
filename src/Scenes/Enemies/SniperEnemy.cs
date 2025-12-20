using FirstPerson.Helpers;
using Godot;

public partial class SniperEnemy : ShootingEnemy
{
    [Export] public float TimeToCharge { get; private set; } = 1;
    public Poll TimeToChargePoll { get; private set; }
    public MeshInstance3D ChargeAura { get; private set; }
    public bool IsCharging { get; private set; }
    public bool ReadyToShoot { get; private set; }
    public Vector3 WhereTargetWillBe { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        TimeToChargePoll = new Poll(TimeToCharge, 0);
        ChargeAura = GetNode<MeshInstance3D>("Hand/ChargeAura");
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        if(!ShouldSkipShooting()) HandleShooting(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleCharging(delta);
    }

    public void HandleCharging(double delta)
    {
        if (!IsShooting && !IsCharging && TargetInLineOfSight && TimeSinceLastShotPoll.IsPollPinged(delta))
        {
            IsCharging = true;
            CalculateWhereTargetWillBe();
            LookAtTarget();
            ChargeAura.Visible = true;
        }

        if (IsCharging && TimeToChargePoll.IsPollPinged(delta))
        {
            ReadyToShoot = true;
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
        if(ReadyToShoot)
        {
            TimeSinceLastShotForMovementPoll.ResetPoll();
            TimeSinceLastShotPoll.ResetPoll();
            ChargeAura.Visible = false;
            IsShooting = true;
            ReadyToShoot = false;
            IsCharging = false;
            var fireBall = FireballPackedScene.Instantiate<Fireball>();
            fireBall.Initialize(WhereTargetWillBe, BulletSpawnPoint.GlobalPosition, ProjectileSpeed);
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

    protected override void LookAtTarget()
    {
        if (IsCharging || IsShooting)
        {
            HelperMethods.RotateForwardToTargetOnYAxis(this, WhereTargetWillBe);
        }
        else
        {
            base.LookAtTarget();
        }
    }

    protected override void HandleRotation()
    {
        if (IsCharging)
        {
            LookAtTarget();
        }
        else
        {
            base.HandleRotation();
        }
    }
}
