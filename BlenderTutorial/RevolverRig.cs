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
    
    public override void PlayIdleAnimation()
    {
        ArmAnimationPlayer.Play("RevolverIdle");
    }

    public override void PlayFireAnimation()
    {
        ArmAnimationPlayer.Play("RevolverHipFire");
    }
}
