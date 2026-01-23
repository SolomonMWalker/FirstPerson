using Godot;
using System;
using FirstPerson.assets.weapons.scripts;
using FirstPerson.Scenes.Player;

public partial class WeaponController : Node
{
    [Export] public CameraController CameraController;
    [Export] public Weapon CurrentWeapon { get; set; }
    [Export] public Node3D WeaponModelParent { get; set; }
    [Export] public WeaponStateMachine WeaponStateMachine { get; set; }
    
    public Node ProjectileParent { get; set; }
    public Node3D CurrentWeaponModel { get; set; }
    public int CurrentAmmo { get; set; }

    public override void _Ready()
    {
        base._Ready();
        if (CurrentWeapon is null)
        {
            GD.PrintErr("Current weapon is null!");
            return;
        }

        ProjectileParent = GetTree().CurrentScene;

        SpawnWeaponModel();
        CurrentAmmo = CurrentWeapon.MaxAmmo;
        CameraController.ShootRaycastLength = (int) CurrentWeapon.Range;
    }

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

    public bool CanFire() => CurrentAmmo > 0;

    public void FireWeapon()
    {
        if (CanFire())
        {
            CurrentAmmo -= 1;
            GD.Print($"Fired! ammo at {CurrentAmmo}");
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

        var hit = CameraController.GetWhatAndWhereShootRaycastIsHitting();
        if (hit.gdObj is not null)
        {
            GD.Print($"Hit {hit.gdObj} at {hit.point}");
            SpawnImpactMarker(hit.point);
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
        var forward = -CameraController.Camera.GlobalTransform.Basis.Z;
        var velocity = forward * CurrentWeapon.ProjectileSpeed;
        projectile.LookAt(projectile.GlobalPosition + forward, Vector3.Up);
        
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
}
