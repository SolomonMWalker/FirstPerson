using Godot;

namespace FirstPerson.scenes.enemies.test;

public class HitInformation(int? healthDamage, int? staggerDamage, Vector3? sourceGlobalPosition)
{
    public int? healthDamage = healthDamage;
    public int? staggerDamage = staggerDamage;
    public Vector3? sourceGlobalPosition = sourceGlobalPosition;
}