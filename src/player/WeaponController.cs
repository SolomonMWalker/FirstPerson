using Godot;
using System;
using FirstPerson.assets.weapons.scripts;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Helpers;
using FirstPerson.scenes.enemies.test;
using FirstPerson.Scenes.Player;
using FirstPerson.utilities;

public partial class WeaponController : Node
{
    [ExportCategory("References")] 
    [Export] public PlayerController PlayerController;
    [Export] public CameraController CameraController;
    [Export] public CameraEffects CameraEffects;
    [Export] public Node3D WeaponModelParent { get; set; }
    [Export] public MouseCaptureComponent MouseCaptureComponent { get; set; }
    [Export] public Reticle Reticle { get; set; }
    //[Export] public Label StateLabel {get; set;}

    [ExportCategory("Idle Sway")]
    [Export] public float IdleSwayFrequency { get; set; } = 0.8f;
    [Export] public Vector2 IdleSwayAmplitube { get; set; } = new Vector2(0.003f, 0.002f);
    [Export] public float IdleSwayStiffness { get; set; } = 40f;
    [Export] public float IdleSwayDamping { get; set; } = 6f;

    [ExportCategory("Mouse Sway")]
    [Export] public float MouseSwayStiffness { get; set; } = 80f;
    [Export] public float MouseSwayDamping { get; set; } = 12f;

    [ExportCategory("Weapon Settings")]
    [Export] public int MaxAngleAccuracyPenalty { get; set; } = 8;
    [Export] public float AimSwayMult { get; set; } = 0.25f;
    
    public Weapon CurrentWeapon { get; set; }
    public WeaponManager WeaponManager { get; set; }
    public Node ProjectileParent { get; set; }
    public WeaponRig CurrentWeaponModel { get; set; }
    public float CurrentAccuracyAnglePenalty { get; private set; }
    public bool Aiming { get; set; }
    private Vector3 _weaponModelParentDefaultPosition, _weaponModelParentDefaultRotation, _baseWeaponPosition;
    private float _idleTime, _idleX, _idleXVelocity, _idleY, _idleYVelocity;
    private float _mouseSwayX, _mouseSwayXVelocity, _mouseSwayY, _mouseSwayYVelocity;

    public override void _Ready()
    {
        base._Ready();
        _weaponModelParentDefaultPosition = WeaponModelParent.Position;
        _weaponModelParentDefaultRotation = WeaponModelParent.Rotation;
        WeaponManager = (WeaponManager) GetTree().GetFirstNodeInGroup("WeaponManager");
        CurrentWeapon = GetCurrentWeaponData().Weapon;
        
        if (CurrentWeapon is null)
        {
            GD.PrintErr("Current weapon is null!");
            return;
        }

        ProjectileParent = GetTree().CurrentScene;

        SpawnWeaponModel();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        SwayWeaponRig(delta);
        CurrentAccuracyAnglePenalty = GetCurrentAccuracyPenalty();
    }

    public WeaponData GetCurrentWeaponData() => WeaponManager.Weapons[WeaponManager.CurrentSlot];
    public int GetCurrentWeaponAmmo() => WeaponManager.GetCurrentAmmo();

    public void SpawnWeaponModel()
    {
        CurrentWeaponModel?.QueueFree();
        if (CurrentWeapon.WeaponModel is not null)
        {
            CurrentWeaponModel = CurrentWeapon.WeaponModel.Instantiate<WeaponRig>();
            WeaponModelParent.AddChild(CurrentWeaponModel);
            CurrentWeaponModel.Position = CurrentWeapon.WeaponPosition;
            CurrentWeaponModel.PlayHipIdleAnimation();
            _baseWeaponPosition = CurrentWeaponModel.Position;
        }
    }

    public void SwayWeaponRig(double delta)
    {
        Vector3 offset = Vector3.Zero;

        if (PlayerController.Idling)
        {
            offset = UpdateIdleSwayOffset((float)delta);
        }
        
        //default pos and rotation of weapon rig are 0
        var fDelta = (float)delta;
        var mouseMovement = MouseCaptureComponent.RawRelativeMouseInput.Clamp(
            CurrentWeapon.SwayMin, CurrentWeapon.SwayMax);
        if (Aiming) mouseMovement *= AimSwayMult;
        var airYOffset = PlayerController.InAir ? PlayerController.Velocity.Y : 0f;
        var mouseSwayResultX = SpringUtility.ApplySpring(
            _mouseSwayX, _mouseSwayXVelocity,
            -mouseMovement.X * CurrentWeapon.SwayAmountPosition * fDelta,
            MouseSwayStiffness, MouseSwayDamping, fDelta);
        _mouseSwayX = mouseSwayResultX.value;
        _mouseSwayXVelocity = mouseSwayResultX.velocity;

        var mouseSwayResultY = SpringUtility.ApplySpring(
            _mouseSwayY, _mouseSwayYVelocity,
            mouseMovement.Y * CurrentWeapon.SwayAmountPosition * fDelta
            + airYOffset * CurrentWeapon.AirSwayAmountPosition,
            MouseSwayStiffness, MouseSwayDamping, fDelta);
        _mouseSwayY = mouseSwayResultY.value;
        _mouseSwayYVelocity = mouseSwayResultY.velocity;
        var rot = WeaponModelParent.Rotation;
        WeaponModelParent.Position = _weaponModelParentDefaultPosition with
        {
            X = _weaponModelParentDefaultPosition.X + _mouseSwayX + offset.X,
            Y = _weaponModelParentDefaultPosition.Y + _mouseSwayY + offset.Y,
        };
        WeaponModelParent.Rotation = rot with
        {
            Y = Mathf.DegToRad(Mathf.Lerp(rot.Y,
                mouseMovement.X * CurrentWeapon.SwayAmountRotationInDeg * fDelta,
                CurrentWeapon.SwaySpeedRotation)),
            X = Mathf.DegToRad(Mathf.Lerp(rot.X,
                -mouseMovement.Y * CurrentWeapon.SwayAmountRotationInDeg * fDelta
                + airYOffset * CurrentWeapon.AirSwayAmountRotationInDeg,
                CurrentWeapon.SwaySpeedRotation)),
        };
    }

    public Vector3 UpdateIdleSwayOffset(float delta)
    {
        if (CurrentWeaponModel is null) return Vector3.Zero;
        
        //increment idle time
        _idleTime += delta;
        
        //calculate sine wave targets (figure 8 pattern)
        var targetX = Mathf.Sin(_idleTime * IdleSwayFrequency) * IdleSwayAmplitube.X;
        var targetY = Mathf.Sin(_idleTime * IdleSwayFrequency * 0.618f) * IdleSwayAmplitube.Y;

        //apply spring
        var resultX = SpringUtility.ApplySpring(
            _idleX, _idleXVelocity, targetX, IdleSwayStiffness,
            IdleSwayDamping, delta);
        _idleX = resultX.value;
        _idleXVelocity = resultX.velocity;
        
        var resultY = SpringUtility.ApplySpring(
            _idleY, _idleYVelocity, targetY, IdleSwayStiffness,
            IdleSwayDamping, delta);
        _idleY = resultY.value;
        _idleYVelocity = resultY.velocity;

        return new Vector3(_idleX, _idleY, 0f);
    }

    public void EndIdleSway()
    {
        _idleTime = 0;
    }

    public bool CanFire()
    {
        return GetCurrentWeaponData().Ammo > 0;
    }

    public bool CanReload()
    {
        return GetCurrentWeaponData().Ammo < CurrentWeapon.MaxAmmo;
    }

    public void AddBullet()
    {
        CurrentWeaponModel.AddBullet();
    }

    public void FireWeapon(bool isAiming)
    {
        if (CanFire())
        {
            if(isAiming)
            {
                CurrentWeaponModel.PlayAimFireAnimation();
            }
            else
            {
                CurrentWeaponModel.PlayHipFireAnimation();
            }
            
            CurrentWeaponModel.FireBullet();
            CameraEffects.AddWeaponKick(
                CurrentWeapon.WeaponKickPitch,
                CurrentWeapon.WeaponKickYaw,
                CurrentWeapon.WeaponKickRoll
            );
            GD.Print($"Fired! ammo at {GetCurrentWeaponAmmo()}");
            
            if (CurrentWeapon.IsHitscan)
            {
                PerformHitscan();
            }
            else
            {
                SpawnProjectile();
            }
        }
    }

    public float GetCurrentAccuracyPenalty()
    {
        //retrieves gun accuracy and player real velocity to calculate accuracy
        var speedPercent = PlayerController.PreviousFrameVelocityLength / (PlayerController.Speed * PlayerController.SprintMovementMult);
        var weaponAccuracyPenalty =
            Aiming ? CurrentWeapon.AccuracyErrorAngleWhenAiming : CurrentWeapon.AccuracyErrorAngle;
        var accuracySpeedPenalty = speedPercent * CurrentWeapon.AccuracyErrorAngleAtMaxMovementSpeed;
        return Mathf.Clamp(weaponAccuracyPenalty + accuracySpeedPenalty, 0, MaxAngleAccuracyPenalty);
    }

    public void PerformHitscan()
    {
        if (CameraController is null)
        {
            GD.PrintErr("No camera controller assigned!");
            return;
        }

        var forward = -CameraController.Camera.GlobalTransform.Basis.Z;
        for (int i = 0; i < CurrentWeapon.PelletCount; i++)
        {
            (GodotObject gdObj, Vector3 point) hit;
            var accuracyXy = new Vector2((float) GD.RandRange(-CurrentAccuracyAnglePenalty, CurrentAccuracyAnglePenalty),
                (float) GD.RandRange(-CurrentAccuracyAnglePenalty, CurrentAccuracyAnglePenalty));
            if (CurrentWeapon.PelletCount > 1)
            {
                if (i != 0)
                {
                    hit = CameraController
                        .GetWhatAndWhereShootRaycastIsHitting(Vector2.Zero, (int) CurrentWeapon.Range);
                }
                else
                {
                    hit = CameraController
                        .GetWhatAndWhereShootRaycastIsHitting(accuracyXy, (int) CurrentWeapon.Range);
                }
            }
            else
            {
                hit = CameraController
                    .GetWhatAndWhereShootRaycastIsHitting(accuracyXy, (int) CurrentWeapon.Range);
            }
            
            if (hit.gdObj is not null)
            {
                //GD.Print($"Hit {hit.gdObj} at {hit.point}");
                this.SpawnImpactMarker(hit.point);
                if (hit.gdObj is Hitbox hitbox)
                {
                    hitbox.Hit(BuildHitInformation(hit.point));
                }
            }
        }
    }

    private HitInformation BuildHitInformation(Vector3 collisionGlobalPosition)
    {
        return new HitInformation(
            healthDamage: (int) CurrentWeapon.Damage,
            staggerDamage: (int) CurrentWeapon.StaggerDamage,
            sourceCombatTargetNode3D: PlayerController,
            sourceGlobalPosition: PlayerController.GlobalPosition,
            collisionGlobalPosition: collisionGlobalPosition
        );
    }

    public void SpawnProjectile()
    {
        if (CurrentWeapon.ProjectileScene is null)
        {
            GD.PrintErr("Current weapon has no projectile scene attached!");
            return;
        }
        
        if (CameraController is null)
        {
            GD.PrintErr("No camera controller assigned!");
            return;
        }

        var projectile = CurrentWeapon.ProjectileScene.Instantiate<Projectile>();
        ProjectileParent.AddChild(projectile);
        
        projectile.GlobalPosition = CameraController.Camera.GlobalPosition;
        var targetMovementRange = CurrentAccuracyAnglePenalty / 10f;
        (float x, float y) accuracyXY = ((float) GD.RandRange(-targetMovementRange, targetMovementRange),
            (float) GD.RandRange(-targetMovementRange, targetMovementRange));
        var forward = -CameraController.Camera.GlobalTransform.Basis.Z;
        var direction = forward + new Vector3(accuracyXY.x, accuracyXY.y, 0) 
            * CameraController.Camera.GlobalTransform.Basis;
        var velocity = direction * CurrentWeapon.ProjectileSpeed;
        projectile.LookAt(projectile.GlobalPosition + direction, Vector3.Up);
        
        projectile.Setup(velocity, CurrentWeapon.Damage);
    }

    public void SwitchWeapon(WeaponData weaponData)
    {
        CurrentWeapon = weaponData.Weapon;
        CurrentWeaponModel?.QueueFree();
        SpawnWeaponModel();
    }
}
