using Godot;
using System;
using FirstPerson.CustomTypes;

public partial class Hitbox : Area3D
{
    private Agent Parent { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Parent = (Agent) GetParent().GetParent();
    }

    public virtual void Hit(HitParameters hitParameters)
    {
        Parent.Hit(hitParameters);
    }
}
