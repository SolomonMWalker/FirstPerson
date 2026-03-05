using Godot;

namespace FirstPerson.scenes.enemies.test;

public class HitInformation(
    float? healthDamage = null, 
    float? staggerDamage = null, 
    Vector3? sourceGlobalPosition = null,
    Vector3? collisionGlobalPosition = null,
    float? pitch = null,
    float? roll = null)
{
    public float? healthDamage = healthDamage;
    public float? staggerDamage = staggerDamage;
    public Vector3? sourceGlobalPosition = sourceGlobalPosition;
    public Vector3? collisionGlobalPosition = collisionGlobalPosition;
    public float? pitch = pitch;
    public float? roll = roll;
}