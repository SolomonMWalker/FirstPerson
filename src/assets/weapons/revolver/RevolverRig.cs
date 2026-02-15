using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class RevolverRig : WeaponRig
{
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
    [Export] public StringName ReloadCloseCylinderAnimationName { get; set; } = "RevolverCloseCylinder";
    [Export] public StringName ReloadInterruptAnimationName { get; set; } = "RevolverReloadInterrupt";

    //upon entrance to these animations, a bullet has been added
    public List<StringName> BulletAddedAnimations { get; private set; } = [];

    public override void _Ready()
    {
        base._Ready();
        BulletAddedAnimations.AddRange([AimToReloadAnimationName, HipToReloadAnimationName, ReloadInsertNextBulletAnimationName]);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }


    public override void InterruptReloadanimation()
    {
        string[] noInterruptAnimations = [ReloadInterruptAnimationName, ReloadCloseCylinderAnimationName, ReloadToCloseCylinderAnimationName];
        if(noInterruptAnimations.Contains<string>(AnimationPlayer.CurrentAnimation)) return;
        AnimationPlayer.ClearQueue();
        AnimationPlayer.Queue(ReloadInterruptAnimationName);
        AnimationPlayer.Queue(ReloadCloseCylinderAnimationName);
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
        CylinderController.RotateCylinderByOneBullet(0.1f);
        AnimationPlayer.Play(HipHammerDownAnimationName);
    }

    public void PlayAimHammerDownAnimation()
    {
        CylinderController.RotateCylinderByOneBullet(0.1f);
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
    
    public override void PlayReloadAnimation(int numberOfBullets)
    {
        if(numberOfBullets <= 0) return;

        AnimationPlayer.Queue(OpenCylinderInsertFirstBulletAnimationName);

        if (numberOfBullets == 1)
        {
            AnimationPlayer.Queue(ReloadToCloseCylinderAnimationName);
            AnimationPlayer.Queue(ReloadCloseCylinderAnimationName);
            return;
        }
        
        List<string> animationsToPlay = [ReloadTurnCylinderAnimationName];
        for (int i = 0; i < numberOfBullets - 1; i++)
        {
            animationsToPlay.AddRange([ReloadInsertNextBulletAnimationName, ReloadTurnCylinderAnimationName]);
        }

        animationsToPlay[^1] = ReloadToCloseCylinderAnimationName;
        animationsToPlay.Add(ReloadCloseCylinderAnimationName);
        animationsToPlay.ForEach(a => AnimationPlayer.Queue(a));
    }
}
