using FirstPerson.scenes.enemies.test;
using Godot;

namespace FirstPerson.agents.enemies.test;

public abstract partial class Enemy : CharacterBody3D
{
    public float Speed { get; set; }
    public Node3D CombatTarget { get; set; }
    public NavigationAgent3D NavigationAgent3D { get; set; }
    
    public abstract void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint);
    public abstract void PostSpawnInitialize(EncounterZone encounterZone);
    public abstract bool HasAffectedBone();
    public abstract void SetAffectedBone(PhysicalBone3D physicalBone3D);
    public abstract void HandleFalling(double delta);
    public abstract void RotateToTarget();
    public abstract bool CanMove();
    public abstract bool CanRotate();
    public abstract void RotateToGlobalPoint(Vector3 globalPoint);
    public abstract void OnVelocityComputed(Vector3 safeVelocity);

}