using Godot;
using System;
using FirstPerson.CustomTypes.StateMachine;

[GlobalClass]
public partial class WeaponParallelState : ParallelState
{
    public WeaponController WeaponController { get; set; }

    public override void _Ready()
    {
        base._Ready();
        WeaponController = (WeaponController)GetTree().GetFirstNodeInGroup("weaponController");
    }
    
    public override void StateProcessing(double delta)
    {
        base.StateProcessing(delta);
        if (WeaponController?.CurrentWeaponModel is null ||
            WeaponController.CurrentWeaponModel.IsAnimationPlaying()) return;
    }
}
