using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    public int health = 10;

    private AnimationPlayer _animationPlayer;
    private bool _queuedForDeath;
    
    public override void _Ready()
    {
        base._Ready();
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        DecreaseHealth(shotParameters.Damage);
        if(!_queuedForDeath && !_animationPlayer.IsPlaying()) _animationPlayer.Play("shot");
    }

    private void DecreaseHealth(int amount)
    {
        health -= amount;
        if (health > 0) return;
        QueueFree();
        _queuedForDeath = true;
    }
}
