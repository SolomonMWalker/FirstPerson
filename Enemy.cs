using Godot;
using System;

public partial class Enemy : ShootableCharacterBody3D
{
    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        GD.Print("shot");
        GetNode<AnimationPlayer>("AnimationPlayer").Play("shot");
    }
}
