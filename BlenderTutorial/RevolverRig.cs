using Godot;
using System;

[GlobalClass]
public partial class WeaponRig : Node3D
{
    public virtual void PlayIdleAnimation()
    {
    }

    public virtual void PlayFireAnimation()
    {
    }
}

[GlobalClass]
public partial class RevolverRig : WeaponRig
{
    [Export] public AnimationPlayer ArmAnimationPlayer;
    [Export] public AnimationPlayer GunAnimationPlayer;
    [Export] public AudioStreamPlayer3D AudioStreamPlayer3D;
    
    public override void PlayIdleAnimation()
    {
        ArmAnimationPlayer.Play("ArmsRevolverIdle");
        GunAnimationPlayer.Play("RevolverIdle");
    }

    public override void PlayFireAnimation()
    {
        AudioStreamPlayer3D.Play();
        ArmAnimationPlayer.Play("ArmsRevolverFire");
        GunAnimationPlayer.Play("RevolverFire");
    }
}
