using Godot;
using System;

[GlobalClass]
public partial class WeaponRig : Node3D
{
    [Export] public AnimationPlayer AnimationPlayer;

    public bool IsAnimationPlaying() => AnimationPlayer.IsPlaying();
    
    public virtual void PlayHipIdleAnimation()
    {
    }

    public virtual void PlayHipFireAnimation()
    {
    }

    public virtual void PlayAimFireAnimation()
    {
    }

    public virtual void PlayHipToAimAnimation()
    {
    }

    public virtual void PlayAimToHipAnimation()
    {
    }
}

[GlobalClass]
public partial class RevolverRig : WeaponRig
{
    [Export] public string HipIdleAnimationName { get; set; }
    [Export] public string HipFireAnimationName { get; set; }
    [Export] public string AimFireAnimationName { get; set; }
    [Export] public string HipToAimAnimationName { get; set; }
    [Export] public string AimToHipAnimationName { get; set; }
    
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
}
