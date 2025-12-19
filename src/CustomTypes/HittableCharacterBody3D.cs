using Godot;

namespace FirstPerson.CustomTypes;

[GlobalClass]
public partial class HittableCharacterBody3D : CharacterBody3D
{
    public virtual void Hit(HitParameters hitParameters) {}
}

public record HitParameters(int HealthDamage = 0, int StaggerDamage = 0, bool IsWeakspot = false);