using Godot;

namespace FirstPerson.scenes.enemies.test;

public class HitInformation(
    int? healthDamage = null, 
    int? staggerDamage = null, 
    Vector3? sourceGlobalPosition = null,
    Vector3? collisionGlobalPosition = null)
{
    public int? healthDamage = healthDamage;
    public int? staggerDamage = staggerDamage;
    public Vector3? sourceGlobalPosition = sourceGlobalPosition;
    public Vector3? collisionGlobalPosition = collisionGlobalPosition;
}