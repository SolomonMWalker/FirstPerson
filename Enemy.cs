using FirstPerson.CustomTypes;
using Godot;

namespace FirstPerson;

public partial class Enemy : ShootableCharacterBody3D
{
    private AnimationPlayer animationPlayer;
    
    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        animationPlayer.Play("shot");
    }
}