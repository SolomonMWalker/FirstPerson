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

    public virtual void PlayFireWhileAimingAnimation()
    {
    }

    public virtual void PlayEnterAimAnimation()
    {
    }

    public virtual void PlayExitAimAnimation()
    {
    }
}

[GlobalClass]
public partial class RevolverRig : WeaponRig
{
    [Export] public AnimationPlayer ArmAnimationPlayer;
    
    public override void PlayIdleAnimation()
    {
        ArmAnimationPlayer.Play("RevolverIdle");
    }

    public override void PlayFireAnimation()
    {
        ArmAnimationPlayer.Play("RevolverHipFire");
    }

    public override void PlayFireWhileAimingAnimation()
    {
        ArmAnimationPlayer.Play("RevolverAimFire");
    }

    public override void PlayEnterAimAnimation()
    {
        ArmAnimationPlayer.Play("RevolverIdleToAim");
    }

    public override void PlayExitAimAnimation()
    {
        ArmAnimationPlayer.Play("RevolverAimToIdle");
    }
}
