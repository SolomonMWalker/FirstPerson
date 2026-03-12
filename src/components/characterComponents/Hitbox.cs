using Godot;
using System;
using FirstPerson.scenes.enemies.test;
using FirstPerson.Scenes.Player;

[GlobalClass]
public partial class Hitbox : Area3D
{
    [Signal]
    public delegate void OnHitSetImpulseReactionEventHandler(float pitch, float roll, Vector3 source);
    [Signal]
    public delegate void OnHitSetCombatTargetEventHandler(Node3D combatTarget);

    public enum Type
    {
        Regular,
        Weakspot
    }

    [Export] public Type HitboxType;
    [Export] public bool IsDebug { get; private set; }

    [ExportCategory("Settings")] 
    [Export] public float WeakspotDamageMult = 1.5f;
    [Export] public float WeakspotStaggerMult = 1.5f;
    
    [ExportCategory("References")]
    [Export] public HealthComponent HealthComponent { get; private set; }
    [Export] public StaggerComponent StaggerComponent { get; private set; }
    [Export] public MeshInstance3D DebugMesh { get; private set; }
    [Export] public Node3D Parent { get; private set; }
    [Export] public PhysicalBone3D AffectedPhysicalBone { get; private set; }

    private Timer _debugMeshTimer;

    public override void _Ready()
    {
        base._Ready();
        if (IsDebug)
        {
            _debugMeshTimer = new Timer();
            _debugMeshTimer.SetWaitTime(1);
            _debugMeshTimer.Autostart = false;
            _debugMeshTimer.Timeout += () =>
            {
                DebugMesh.SetVisible(false);
            };
            AddChild(_debugMeshTimer);
        }
    }

    public void Hit(HitInformation hitInformation)
    {
        if (hitInformation.pitch.HasValue && hitInformation.roll.HasValue && hitInformation.sourceGlobalPosition is not null)
        {
            EmitSignalOnHitSetImpulseReaction(hitInformation.pitch.Value, hitInformation.roll.Value, hitInformation.sourceGlobalPosition.Value);
        }
        
        if (hitInformation.healthDamage.HasValue)
        {
            var damage = hitInformation.healthDamage.Value;
            if (HitboxType == Type.Weakspot)
            {
                damage *= WeakspotDamageMult;
            }
            HealthComponent?.DepleteHealth(damage);
            if(IsDebug) DebugHit();
            if (Parent is Grunt grunt && AffectedPhysicalBone is not null)
            {
                grunt.affectedBone = AffectedPhysicalBone;
                if (hitInformation.sourceGlobalPosition is not null && hitInformation.collisionGlobalPosition.HasValue
                                                                    && HealthComponent?.CurrentHealth > 0)
                {
                    grunt.SetLastDamageDirection(hitInformation.sourceGlobalPosition.Value, 
                        hitInformation.collisionGlobalPosition.Value);
                }
            }
        }

        if (hitInformation.staggerDamage.HasValue)
        {
            var damage = hitInformation.staggerDamage.Value;
            if (HitboxType == Type.Weakspot)
            {
                damage *= WeakspotStaggerMult;
            }
            StaggerComponent?.DepleteStagger(damage);
        }

        if (hitInformation.sourceCombatTargetNode3D is not null)
        {
            EmitSignalOnHitSetCombatTarget(hitInformation.sourceCombatTargetNode3D);
        }
    }

    public void DebugHit()
    {
        _debugMeshTimer.Stop();
        DebugMesh.SetVisible(true);
        _debugMeshTimer.Start();
    }
}
