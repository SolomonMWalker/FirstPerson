using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    public int health = 10;

    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        base._Ready();
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        if(!_animationPlayer.IsPlaying()) _animationPlayer.Play("shot");
        DecreaseHealth(shotParameters.Damage);
    }

    public void DecreaseHealth(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            QueueFree();
        }
    }
}
