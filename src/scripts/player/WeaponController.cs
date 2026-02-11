using Godot;
using System;
using FirstPerson.assets.weapons.scripts;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Helpers;
using FirstPerson.Scenes.Player;

public partial class WeaponController : Node
{
    [ExportCategory("References")] 
    [Export] public PlayerController Player;
    [Export] public CameraController CameraController;
    [Export] public Node3D WeaponModelParent { get; set; }
    [Export] public MouseCaptureComponent MouseCaptureComponent { get; set; }
    [Export] public Reticle Reticle { get; set; }

    [ExportCategory("Weapon Settings")]
    [Export] public int MaxAngleAccuracyPenalty { get; set; } = 8;
    
    public Weapon CurrentWeapon { get; set; }
    public WeaponManager WeaponManager { get; set; }
    public Node ProjectileParent { get; set; }
    public WeaponRig CurrentWeaponModel { get; set; }
    public float CurrentAccuracyAnglePenalty { get; private set; }
    public double FireRateTimer { get; private set; }
    public bool CanFireNextRound { get; private set; } = true;
    public bool Aiming { get; private set; } = true;

    private (bool, string) _bulletAddedAnimStartedAndName;
    private Vector3 WeaponModelParentDefaultPosition, WeaponModelParentDefaultRotation;

    public override void _Ready()
    {
        base._Ready();
        WeaponModelParentDefaultPosition = WeaponModelParent.Position;
        WeaponModelParentDefaultRotation = WeaponModelParent.Rotation;
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

    public override void _Process(double delta)
    {
        base._Process(delta);
        CheckForBulletAdd();
        if (FireRateTimer > 0)
        {
            FireRateTimer -= delta;
            if (FireRateTimer <= 0)
            {
                CanFireNextRound = true;
            }
        }
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
        }
    }

    public void SwayWeaponRig(double delta)
    {
        //default pos and rotation of weapon rig are 0
        var fDelta = (float)delta;
        var mouseMovement =
            MouseCaptureComponent.RawRelativeMouseInput.Clamp(CurrentWeapon.SwayMin, CurrentWeapon.SwayMax);
        var pos = WeaponModelParent.Position;
        var rot = WeaponModelParent.Rotation;
        WeaponModelParent.Position = pos with
        {
            X = Mathf.Lerp(pos.X, 
                -mouseMovement.X * CurrentWeapon.SwayAmountPosition * fDelta,
                CurrentWeapon.SwaySpeedPosition),
            Y = Mathf.Lerp(pos.Y, 
                mouseMovement.Y * CurrentWeapon.SwayAmountPosition * fDelta,
                CurrentWeapon.SwaySpeedPosition),
        };
        WeaponModelParent.Rotation = rot with
        {
            Y = Mathf.DegToRad(Mathf.Lerp(rot.Y, 
                mouseMovement.X * CurrentWeapon.SwayAmountRotationInDeg * fDelta,
                CurrentWeapon.SwaySpeedRotation)),
            X = Mathf.DegToRad(Mathf.Lerp(rot.X, 
                -mouseMovement.Y * CurrentWeapon.SwayAmountRotationInDeg * fDelta,
                CurrentWeapon.SwaySpeedRotation)),
        };
    }

    public bool CanFire()
    {
        return GetCurrentWeaponData().Ammo > 0 && CanFireNextRound;
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
            
            WeaponManager.UseAmmo(WeaponManager.CurrentSlot);
            GD.Print($"Fired! ammo at {GetCurrentWeaponAmmo()}");

            CanFireNextRound = false;
            FireRateTimer = 1.0 / CurrentWeapon.FireRatePerSecond;
            
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
        var speedPercent = Player.Velocity.Length() / (Player.Speed * Player.SprintMovementMult);
        var accuracySpeedPenalty = speedPercent * CurrentWeapon.AccuracyErrorAngleAtMaxMovementSpeed;
        return Mathf.Clamp(CurrentWeapon.AccuracyErrorAngle + accuracySpeedPenalty, 0, MaxAngleAccuracyPenalty);
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
            var direction = forward * CameraController.Camera.GlobalTransform.Basis;
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
            }
        }
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

    public void InterruptReloadanimation()
    {
        _bulletAddedAnimStartedAndName = (false, null);
    }

    //kinda hacky, but allows me to keep the imported animation player intact
    //and keeps me from having to chain them
    public void CheckForBulletAdd()
    {
        if (CurrentWeaponModel is not RevolverRig revolverRig) return;
        if (!_bulletAddedAnimStartedAndName.Item1
            && revolverRig.BulletAddedAnimations.Contains(revolverRig.AnimationPlayer.CurrentAnimation)
        ) {
            _bulletAddedAnimStartedAndName.Item1 = true;
            _bulletAddedAnimStartedAndName.Item2 = revolverRig.AnimationPlayer.CurrentAnimation;
            return;
        }

        if
        (_bulletAddedAnimStartedAndName.Item1 && 
            (
                !revolverRig.AnimationPlayer.IsPlaying() ||
                revolverRig.AnimationPlayer.CurrentAnimation != _bulletAddedAnimStartedAndName.Item2
            )        
        )
        {
            WeaponManager.Weapons[WeaponManager.CurrentSlot].Ammo += 1;
            GD.Print($"Ammo added, current ammo is {WeaponManager.Weapons[WeaponManager.CurrentSlot].Ammo}");
            _bulletAddedAnimStartedAndName.Item1 = false;
            _bulletAddedAnimStartedAndName.Item2 = null;
        }
        
    }
}
