using Godot;

[GlobalClass]
public partial class WeaponRig : Node3D
{
    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public WeaponStateMachine WeaponStateMachine;
    public WeaponController WeaponController;
    public float AimSwayMultiplier { get; protected set; } = 1f;

    public override void _Ready()
    {
        base._Ready();
        WeaponController = (WeaponController) GetTree().GetFirstNodeInGroup("weaponController");
    }


    public bool IsAnimationPlaying() => AnimationPlayer.IsPlaying();

    public virtual void StartAiming() {}
    public virtual void StopAiming() {}
    public virtual void AddBullet() {}
    public virtual void FireBullet() {}
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