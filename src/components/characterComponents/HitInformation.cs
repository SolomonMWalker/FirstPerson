using Godot;

namespace FirstPerson.scenes.enemies.test;

public enum HitType
{
    Regular,
    Explosion,
    Hazard, //might be replaced with specific hazard later
}

public class HitInformation(
    HitType hitType = HitType.Regular,
    float? healthDamage = null, 
    float? staggerDamage = null, 
    Node3D sourceCombatTargetNode3D = null,
    Vector3? sourceGlobalPosition = null,
    Vector3? collisionGlobalPosition = null,
    float? pitch = null,
    float? roll = null)
{
    public HitType HitType = HitType.Regular;
    public float? HealthDamage = healthDamage;
    public float? StaggerDamage = staggerDamage;
    public readonly Node3D SourceCombatTargetNode3D = sourceCombatTargetNode3D;
    public Vector3? SourceGlobalPosition = sourceGlobalPosition;
    public Vector3? CollisionGlobalPosition = collisionGlobalPosition;
    public float? Pitch = pitch;
    public float? Roll = roll;

}