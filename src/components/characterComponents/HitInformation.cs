using Godot;

namespace FirstPerson.scenes.enemies.test;

public class HitInformation(
    int? healthDamage = null, 
    int? staggerDamage = null, 
    Node3D source = null,
    Vector3? collisionGlobalPosition = null,
    float? pitch = null,
    float? roll = null)
{
    public int? healthDamage = healthDamage;
    public int? staggerDamage = staggerDamage;
    public Node3D source = source;
    public Vector3? collisionGlobalPosition = collisionGlobalPosition;
    public float? pitch = pitch;
    public float? roll = roll;
}