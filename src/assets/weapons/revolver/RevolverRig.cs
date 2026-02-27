using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using FirstPerson.Scenes.Player;

[GlobalClass]
public partial class RevolverRig : WeaponRig
{
    [ExportCategory("References")]
    [Export] public PackedScene EjectedCasingScene { get; set; }
    [Export] public CylinderController CylinderController { get; set; }
    [Export] public AudioStreamPlayer3D AudioStreamPlayer3D { get; set; }
    
    [ExportCategory("AnimationNames")]
    [Export] public StringName HipIdleAnimationName { get; set; } = "RevolverHipIdle";
    [Export] public StringName HipFireAnimationName { get; set; } = "RevolverHipFire";
    [Export] public StringName AimFireAnimationName { get; set; } = "RevolverAimFire";
    [Export] public StringName AimHammerDownAnimationName { get; set; } = "RevolverAimHammerDown";
    [Export] public StringName HipHammerDownAnimationName { get; set; } = "RevolverHipHammerDown";
    [Export] public StringName HipToAimAnimationName { get; set; } = "RevolverHipToAim";
    [Export] public StringName AimToHipAnimationName { get; set; } = "RevolverAimToHip";
    [Export] public StringName HammerDownHipToAimAnimationName { get; set; } = "RevolverHammerDownHipToAim";
    [Export] public StringName HammerDownAimToHipAnimationName { get; set; } = "RevolverHammerDownAimToHip";
    [Export] public StringName AimToReloadAnimationName { get; set; } = "RevolverReloadFromAimOpenCylinder";
    [Export] public StringName HipToReloadAnimationName { get; set; } = "RevolverReloadFromHipOpenCylinder";
    [Export] public StringName OpenCylinderInsertFirstBulletAnimationName { get; set; } = "RevolverReloadOpenCylinderInsertFirstBullet";
    [Export] public StringName ReloadInsertNextBulletAnimationName { get; set; } = "RevolverReloadInsertNextBullet";
    [Export] public StringName ReloadTurnCylinderAnimationName { get; set; } = "RevolverReloadTurnCylinder";
    [Export] public StringName ReloadToCloseCylinderAnimationName { get; set; } = "RevolverReloadToCloseCylinder";
    [Export] public StringName ReloadCloseCylinderStartAnimationName { get; set; } = "RevolverCloseCylinderStart";
    [Export] public StringName ReloadCloseCylinderEndAnimationName { get; set; } = "RevolverCloseCylinderEnd";
    [Export] public StringName ReloadInterruptAnimationName { get; set; } = "RevolverReloadInterrupt";

    //on interrupt of reload, if pressed before cylinder turn, full interrupt
    //otherwise, just reload that bullet and end the animation
    public readonly List<StringName> InterruptibleReloadAnimations = [];
    public readonly List<StringName> PostCylinderTurnInterruptReloadPath = [];

    private Node3D _projectileParent;
    private PlayerController _playerController;
    private readonly Dictionary<int, MeshInstance3D> _bulletCasings = [];
    private readonly Dictionary<int, MeshInstance3D> _bullets = [];
    private bool _isHammerDown;

    public override void _Ready()
    {
        base._Ready();
        _projectileParent = (Node3D) GetTree().GetFirstNodeInGroup("projectileParent");
        _playerController = (PlayerController) GetTree().GetFirstNodeInGroup("player");
        InterruptibleReloadAnimations.AddRange([
            AimToReloadAnimationName,
            HipToReloadAnimationName,
            OpenCylinderInsertFirstBulletAnimationName,
            ReloadInsertNextBulletAnimationName
        ]);
        PostCylinderTurnInterruptReloadPath.AddRange([
            ReloadTurnCylinderAnimationName,
            ReloadToCloseCylinderAnimationName,
            ReloadCloseCylinderStartAnimationName,
            ReloadCloseCylinderEndAnimationName
        ]);

        foreach (var child in CylinderController.CylinderParent.GetChildren().OfType<MeshInstance3D>())
        {
            var name = child.Name.ToString();
            if (name.Contains("bulletWithCasing"))
            {
                var numberChar = name.ToCharArray().Where(char.IsDigit).First().ToString();
                var number = Int16.Parse(numberChar);
                _bullets.Add(number, child);
            }
            else if (name.Contains("bulletCasing"))
            {
                var numberChar = name.ToCharArray().Where(char.IsDigit).First().ToString();
                var number = Int16.Parse(numberChar);
                _bulletCasings.Add(number, child);
            }
        }
    }
    
    public override void InterruptReloadanimation()
    {
        string[] noInterruptAnimations = [ReloadInterruptAnimationName, ReloadCloseCylinderStartAnimationName, ReloadToCloseCylinderAnimationName];
        if(noInterruptAnimations.Contains<string>(AnimationPlayer.CurrentAnimation)) return;
        AnimationPlayer.ClearQueue();
        AnimationPlayer.Queue(ReloadInterruptAnimationName);
        AnimationPlayer.Queue(ReloadCloseCylinderStartAnimationName);
    }

    public override void PlayHipIdleAnimation()
    {
        AnimationPlayer.Play(HipIdleAnimationName);
    }

    public override void PlayHipFireAnimation()
    {
        AnimationPlayer.Play(HipFireAnimationName);
    }

    public override void PlayAimFireAnimation()
    {
        AnimationPlayer.Play(AimFireAnimationName);
    }

    public override void PlayHipToAimAnimation()
    {
        AnimationPlayer.Play(HipToAimAnimationName);
    }

    public override void PlayAimToHipAnimation()
    {
        AnimationPlayer.Play(AimToHipAnimationName);
    }
    
    public void PlayHammerDownAimToHipAnimation()
    {
        
        AnimationPlayer.Play(HammerDownAimToHipAnimationName);
    }

    public void PlayHammerDownHipToAimAnimation()
    {
        AnimationPlayer.Play(HammerDownHipToAimAnimationName);
    }

    public override void PlayAimToReloadAnimation()
    {
        AnimationPlayer.Play(AimToReloadAnimationName);
    }

    public override void PlayHipToReloadAnimation()
    {
        AnimationPlayer.Play(HipToReloadAnimationName);
    }

    public void PlayHipHammerDownAnimation()
    {
        AnimationPlayer.Play(HipHammerDownAnimationName);
    }

    public void PlayAimHammerDownAnimation()
    {
        AnimationPlayer.Play(AimHammerDownAnimationName);
    }

    public override void AddBullet()
    {
        WeaponController.WeaponManager.AddAmmo(WeaponController.WeaponManager.CurrentSlot);
        _bulletCasings[WeaponController.WeaponManager.GetCurrentAmmo()].SetVisible(false);
        _bullets[WeaponController.WeaponManager.GetCurrentAmmo()].SetVisible(true);
    }
    
    public override void FireBullet()
    {
        var ammo = WeaponController.WeaponManager.GetCurrentWeapon().Ammo;
        _bulletCasings[ammo].SetVisible(true);
        _bullets[ammo].SetVisible(false);
        WeaponController.WeaponManager.UseAmmo(WeaponController.WeaponManager.CurrentSlot);
        AudioStreamPlayer3D.Play();
    }
    
    public void ReloadEjectCasings()
    {
        var casingsToEject = _bulletCasings
            .Where(kvp => kvp.Key > WeaponController.WeaponManager.GetCurrentAmmo())
            .Where(kvp => kvp.Value.Visible);
        foreach (var kvp in casingsToEject)
        {
            kvp.Value.Visible = false;
            var casing = EjectedCasingScene.Instantiate<RigidBody3D>();
            _projectileParent.AddChild(casing);
            casing.GlobalTransform = kvp.Value.GlobalTransform;
            casing.GlobalPosition -= casing.GlobalBasis.Y * 0.02f;
            casing.LinearVelocity = -casing.GlobalBasis.Y * 2;
        }
    }

    public void TurnCylinderHammerdown()
    {
        CylinderController.RotateCylinderHammerDown(0.2f, WeaponController.WeaponManager.GetCurrentAmmo());
    }

    public void TurnCylinderStartReload()
    {
        CylinderController.RotateCylinderOpenCylinder(0.2f, WeaponController.WeaponManager.GetCurrentAmmo());
    }

    public void TurnCylinderReloadNext()
    {
        CylinderController.RotateCylinderReloadTurn(0.2f, WeaponController.WeaponManager.GetCurrentAmmo());
    }

    public void TurnCylinderEndReload()
    {
        CylinderController.RotateCylinderCloseCylinder(0.2f, WeaponController.WeaponManager.GetCurrentAmmo());
    }
    
    public override void PlayReloadAnimation(int numberOfBullets)
    {
        AnimationPlayer.Queue(OpenCylinderInsertFirstBulletAnimationName);
        
        if (numberOfBullets == 1)
        {
            AnimationPlayer.Queue(ReloadToCloseCylinderAnimationName);
            AnimationPlayer.Queue(ReloadCloseCylinderStartAnimationName);
            return;
        }
        
        List<string> animationsToPlay = [ReloadTurnCylinderAnimationName];
        for (int i = 0; i < numberOfBullets - 1; i++)
        {
            animationsToPlay.AddRange([ReloadInsertNextBulletAnimationName, ReloadTurnCylinderAnimationName]);
        }

        animationsToPlay[^1] = ReloadToCloseCylinderAnimationName;
        animationsToPlay.Add(ReloadCloseCylinderStartAnimationName);
        animationsToPlay.ForEach(a => AnimationPlayer.Queue(a));
    }

    public override void StartAiming()
    {
        WeaponController.Aiming = true;
        _playerController.Reticle.Visible = false;
        _playerController.CameraEffects.Aiming = true;
        _playerController.CameraController.EnterAimTweenActivate();
    }

    public override void StopAiming()
    {
        WeaponController.Aiming = false;
        _playerController.Reticle.Visible = true;
        _playerController.CameraEffects.Aiming = false;
        _playerController.CameraController.ExitAimTweenActivate();
    }
}
