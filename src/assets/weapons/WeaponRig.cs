using Godot;

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