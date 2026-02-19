using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class RevolverRig : WeaponRig
{
    [Export] public PackedScene EjectedCasingScene;
    [Export] public Node3D ForwardNode;
    [Export] public CylinderController CylinderController;
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
    public List<StringName> InterruptibleReloadAnimations = [];
    public List<StringName> PostCylinderTurnInterruptReloadPath = [];

    private Node3D ProjectileParent;
    private Dictionary<int, MeshInstance3D> _bulletCasings = [];
    private Dictionary<int, MeshInstance3D> _bullets = [];
    private bool _isHammerDown;

    public override void _Ready()
    {
        base._Ready();
        ProjectileParent = (Node3D) GetTree().GetFirstNodeInGroup("projectileParent");
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

    public void PlayHammerDownAimToHipAnimation()
    {
        AnimationPlayer.Play(HammerDownAimToHipAnimationName);
    }

    public void PlayHammerDownHipToAimAnimation()
    {
        AnimationPlayer.Play(HammerDownHipToAimAnimationName);
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
        GD.Print($"ammo at {ammo}, does bulletCasing exist? {_bulletCasings[ammo] != null}");
        _bulletCasings[ammo].SetVisible(true);
        _bullets[ammo].SetVisible(false);
        WeaponController.WeaponManager.UseAmmo(WeaponController.WeaponManager.CurrentSlot);
    }
    
    public void ReloadEjectCasings()
    {
        var casingsToEject = _bulletCasings.Where(kvp => kvp.Key > WeaponController.WeaponManager.GetCurrentAmmo());
        casingsToEject.ToList().ForEach(casing => casing.Value.Visible = false);
        foreach (var kvp in casingsToEject)
        {
            var casing = EjectedCasingScene.Instantiate<RigidBody3D>();
            ProjectileParent.AddChild(casing);
            casing.GlobalTransform = kvp.Value.GlobalTransform;
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
}
