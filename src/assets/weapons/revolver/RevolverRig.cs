using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class WeaponRig : Node3D
{
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public WeaponStateMachine WeaponStateMachine;

    public bool IsAnimationPlaying() => AnimationPlayer.IsPlaying();
    
    public virtual void PlayHipIdleAnimation() {}
    public virtual void PlayHipFireAnimation() {}
    public virtual void PlayAimFireAnimation() {}
    public virtual void PlayHipToAimAnimation() {}
    public virtual void PlayAimToHipAnimation() {}
    public virtual void PlayAimToReloadAnimation() {}
    public virtual void PlayHipToReloadAnimation() {}
    public virtual void PlayReloadAnimation(int numberOfBullets) {}
    public virtual void InterruptReloadanimation() {}
}

[GlobalClass]
public partial class RevolverRig : WeaponRig
{
    [Export] public StringName HipIdleAnimationName { get; set; } = "RevolverHipIdle";
    [Export] public StringName HipFireAnimationName { get; set; } = "RevolverHipFire";
    [Export] public StringName AimFireAnimationName { get; set; } = "RevolverAimFire";
    [Export] public StringName AimHammerDownAnimationName { get; set; } = "RevolverAimHammerDown";
    [Export] public StringName HipHammerDownAnimationName { get; set; } = "RevolverHipHammerDown";
    [Export] public StringName HipToAimAnimationName { get; set; } = "RevolverHipToAim";
    [Export] public StringName AimToHipAnimationName { get; set; } = "RevolverAimToHip";
    [Export] public StringName HammerDownHipToAimAnimationName { get; set; } = "RevolverHammerDownHipToAim";
    [Export] public StringName HammerDownAimToHipAnimationName { get; set; } = "RevolverHammerDownAimToHip";
    [Export] public StringName AimToReloadAnimationName { get; set; } = "RevolverReloadFromAimOpenCylinderInsertFirstBullet";
    [Export] public StringName HipToReloadAnimationName { get; set; } = "RevolverReloadFromHipOpenCylinderInsertFirstBullet";
    [Export] public StringName ReloadInsertNextBulletAnimationName { get; set; } = "RevolverReloadInsertNextBullet";
    [Export] public StringName ReloadTurnCylinderAnimationName { get; set; } = "RevolverReloadTurnCylinder";
    [Export] public StringName ReloadCloseCylinderAnimationName { get; set; } = "RevolverReloadCloseCylinder";
    [Export] public StringName ReloadInterruptAnimationName { get; set; } = "RevolverReloadInterrupt";

    //upon entrance to these animations, a bullet has been added
    public List<StringName> BulletAddedAnimations { get; private set; } = [];

    public override void _Ready()
    {
        base._Ready();
        BulletAddedAnimations.AddRange([AimToReloadAnimationName, HipToReloadAnimationName, ReloadInsertNextBulletAnimationName]);
    }

    public override void InterruptReloadanimation()
    {
        if(AnimationPlayer.CurrentAnimation == ReloadCloseCylinderAnimationName) return;
        AnimationPlayer.ClearQueue();
        AnimationPlayer.Play(ReloadInterruptAnimationName);
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
    
    public override void PlayReloadAnimation(int numberOfBullets)
    {
        if(numberOfBullets <= 0) return;

        if (numberOfBullets == 1)
        {
            AnimationPlayer.Queue(ReloadCloseCylinderAnimationName);
            return;
        }
        
        List<string> animationsToPlay = [ReloadTurnCylinderAnimationName];
        for (int i = 0; i < numberOfBullets - 1; i++)
        {
            animationsToPlay.AddRange([ReloadInsertNextBulletAnimationName, ReloadTurnCylinderAnimationName]);
        }

        animationsToPlay[^1] = ReloadCloseCylinderAnimationName;
        animationsToPlay.ForEach(a => AnimationPlayer.Queue(a));
    }
}
