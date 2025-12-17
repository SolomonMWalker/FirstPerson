using Godot;
using System;

public partial class Weakspot : Area3D
{
    private Agent Parent { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Parent = (Agent) GetParent().GetParent();
    }

    public virtual void Hit(HitParameters hitParameters)
    {
        Parent.Hit(hitParameters with { IsWeakspot = true });
    }
}
