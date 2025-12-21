using Godot;

namespace FirstPerson.CustomTypes;

[GlobalClass]
public partial class HittableCharacterBody3D : CharacterBody3D
{
    public virtual void Hit(HitParameters hitParameters) {}
    public virtual Vector3 GetCenter() => GlobalPosition;
}

public record HitParameters(int HealthDamage = 0, int StaggerDamage = 0, bool IsWeakspot = false);