using Godot;
using System;
using FirstPerson.assets.weapons.scripts;
using FirstPerson.CustomTypes.StateMachine;
using FirstPerson.Scenes.Player;

public partial class WeaponController : Node
{
    [Export] public CameraController CameraController;
    [Export] public Node3D WeaponModelParent { get; set; }
    [Export] public WeaponStateMachine WeaponStateMachine { get; set; }
    
    public Weapon CurrentWeapon { get; set; }
    public WeaponManager WeaponManager { get; set; }
    public Node ProjectileParent { get; set; }
    public Node3D CurrentWeaponModel { get; set; }
    public double FireRateTimer { get; private set; }
    public bool CanFireNextRound { get; private set; } = true;

    public override void _Ready()
    {
        base._Ready();
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
        if (FireRateTimer > 0)
        {
            FireRateTimer -= delta;
            if (FireRateTimer <= 0)
            {
                CanFireNextRound = true;
            }
        }
    }

    public WeaponData GetCurrentWeaponData() => WeaponManager.Weapons[WeaponManager.CurrentSlot];
    public int GetCurrentWeaponAmmo() => GetCurrentWeaponData().Ammo;

    public void SpawnWeaponModel()
    {
        CurrentWeaponModel?.QueueFree();
        if (CurrentWeapon.WeaponModel is not null)
        {
            CurrentWeaponModel = CurrentWeapon.WeaponModel.Instantiate<Node3D>();
            WeaponModelParent.AddChild(CurrentWeaponModel);
            CurrentWeaponModel.Position = CurrentWeapon.WeaponPosition;
        }
    }

    public bool CanFire()
    {
        return GetCurrentWeaponData().Ammo > 0 && CanFireNextRound;
    }

    public void FireWeapon()
    {
        if (CanFire())
        {
            WeaponManager.UseAmmo(WeaponManager.CurrentSlot);
            GD.Print($"Fired! ammo at {GetCurrentWeaponData().Ammo}");

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

    public void PerformHitscan()
    {
        if (CameraController is null)
        {
            GD.PrintErr("No camera controller assigned!");
            return;
        }

        var forward = -CameraController.Camera.GlobalTransform.Basis.Z;
        var accuracySpread = (100 - CurrentWeapon.Accuracy) / 1000.0f;

        for (int i = 0; i < CurrentWeapon.PelletCount; i++)
        {
            (float x, float y) accuracyXY = ((float) GD.RandRange(-accuracySpread, accuracySpread),
                (float) GD.RandRange(-accuracySpread, accuracySpread));
            var direction = forward + new Vector3(accuracyXY.x, accuracyXY.y, 0) * CameraController.Camera.GlobalTransform.Basis;
            if (CurrentWeapon.PelletCount > 1)
            {
                (float x, float y) spreadXY = ((float) GD.RandRange(-CurrentWeapon.SpreadAngle, CurrentWeapon.SpreadAngle),
                    (float) GD.RandRange(-CurrentWeapon.SpreadAngle, CurrentWeapon.SpreadAngle));
                direction += new Vector3(spreadXY.x, spreadXY.y, 0) * CameraController.Camera.GlobalTransform.Basis;
            }
            var to = CameraController.Camera.GlobalPosition + direction * CurrentWeapon.Range;
            var hit = CameraController
                .GetWhatAndWhereShootRaycastIsHitting(to);
            if (hit.gdObj is not null)
            {
                //GD.Print($"Hit {hit.gdObj} at {hit.point}");
                SpawnImpactMarker(hit.point);
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
        
        var accuracySpread = (100 - CurrentWeapon.Accuracy) / 1000.0f;
        (float x, float y) accuracyXY = ((float) GD.RandRange(-accuracySpread, accuracySpread),
            (float) GD.RandRange(-accuracySpread, accuracySpread));
        var forward = -CameraController.Camera.GlobalTransform.Basis.Z;
        var direction = forward + new Vector3(accuracyXY.x, accuracyXY.y, 0) 
            * CameraController.Camera.GlobalTransform.Basis;
        var velocity = direction * CurrentWeapon.ProjectileSpeed;
        projectile.LookAt(projectile.GlobalPosition + direction, Vector3.Up);
        
        projectile.Setup(velocity, CurrentWeapon.Damage);
    }
    
    public void SpawnImpactMarker(Vector3 position)
    {
        var marker = new MeshInstance3D();
        var box = new BoxMesh();
        box.Size = new Vector3(0.1f, 0.1f, 0.1f);
        marker.Mesh = box;

        var material = new StandardMaterial3D();
        material.AlbedoColor = Colors.Red;
        marker.SetSurfaceOverrideMaterial(0, material);

        GetTree().CurrentScene.AddChild(marker);
        marker.GlobalPosition = position;

        GetTree().CreateTimer(2.0).Timeout += marker.QueueFree;
    }

    public void SwitchWeapon(WeaponData weaponData)
    {
        CurrentWeapon = weaponData.Weapon;
        CurrentWeaponModel?.QueueFree();
        SpawnWeaponModel();
        WeaponStateMachine.HandleChangeStateEvent(this, new ChangeStateEventArgs("WeaponIdleState"));
    }
}
