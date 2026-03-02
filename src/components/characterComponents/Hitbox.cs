using Godot;
using System;
using FirstPerson.scenes.enemies.test;

[GlobalClass]
public partial class Hitbox : Area3D
{
    [Export] public bool IsDebug { get; private set; }
    
    [ExportCategory("References")]
    [Export] public HealthComponent HealthComponent { get; private set; }
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
        if (hitInformation.healthDamage.HasValue)
        {
            HealthComponent?.DepleteHealth(hitInformation.healthDamage.Value);
            if(IsDebug) DebugHit();
            if (Parent is Grunt grunt && AffectedPhysicalBone is not null)
            {
                grunt.affectedBone = AffectedPhysicalBone;
                if (hitInformation.source is not null && hitInformation.collisionGlobalPosition.HasValue)
                {
                    grunt.SetLastDamageDirection(hitInformation.source, 
                        hitInformation.collisionGlobalPosition.Value);
                }
            }
        }
    }

    public void DebugHit()
    {
        _debugMeshTimer.Stop();
        DebugMesh.SetVisible(true);
        _debugMeshTimer.Start();
    }
}
