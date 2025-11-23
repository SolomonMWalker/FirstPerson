using Godot;

public partial class Enemy : ShootableCharacterBody3D
{
    public int health = 10;

    public override void Shot(ShotParameters shotParameters)
    {
        base.Shot(shotParameters);
        GetNode<AnimationPlayer>("AnimationPlayer").Play("shot");
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
