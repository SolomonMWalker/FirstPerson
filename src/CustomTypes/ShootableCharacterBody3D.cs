using Godot;

[GlobalClass]
public partial class ShootableCharacterBody3D : CharacterBody3D
{
    public virtual void Shot(ShotParameters shotParameters) {}
}

public record ShotParameters(int Damage);