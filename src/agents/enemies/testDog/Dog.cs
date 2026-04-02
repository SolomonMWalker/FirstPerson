using Godot;
using System;
using System.Collections.Generic;

public partial class Dog : CombatAgent
{
    [ExportCategory("Dog Settings")]
    [Export] public float AttackRate { get; set; } = 2.0f;
    [Export] public float LeapAttackSpeed { get; set; } = 5.0f;

    [ExportCategory("Components")]
    [Export] public AgentZigzagComponent AgentZigzagComponent { get; set; }
    [Export] public AgentFollowComponent AgentFollowComponent { get; set; }
    [Export] public AgentStopComponent AgentStopComponent { get; set; }
    
    [ExportCategory("References")]
    [Export] public CustomAnimationTree CustomAnimationTree { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public ShapeCast3D ShouldCloseAttackShapeCast { get; set; }
    [Export] public ShapeCast3D CloseAttackRangeShapeCast { get; set; }
    [Export] public FuzzyStartTimer AttackRateTimer { get; set; }

    public bool meleeAttacking, leapAttacking, nextAttackIsLeap;
    public bool Attacking { get => _attacking; }
    public bool RotationEnabled { get; set; }
    private bool _attacking;
    private List<GodotObject> meleeAttackObjectsHit = [];
    public override bool CanRotate() => base.CanRotate() && RotationEnabled;
    public void DisableRotation() => RotationEnabled = false;
    public void EnableRotation() => RotationEnabled = true;
    public void StartAttacking() => _attacking = true;

    public void StopAttacking()
    {
        _attacking = false;
        CustomAnimationTree.TrySetParam("stationaryAttack", false);
        CloseAttackRangeShapeCast.Enabled = false;
    }

    public void EnableMeleeHitbox() => CloseAttackRangeShapeCast.Enabled = true;
    public void DisableMeleeHitbox() => CloseAttackRangeShapeCast.Enabled = false;
    
    public override void SetLastDamageDirection(Vector3 sourceGlobalPosition, Vector3 collisionGlobalPoint)
    {
        base.SetLastDamageDirection(sourceGlobalPosition, collisionGlobalPoint);
        CustomAnimationTree.TrySetParam("blend_position", dirLastDamageXz);
        CustomAnimationTree.TrySetParam("request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
    
    public override void _Ready()
    {
        base._Ready();
        defaultCombatAi = AgentZigzagComponent;
        defaultNoncombatAi = AgentStopComponent;
        CurrentNavComponent = AgentStopComponent;

        AttackRateTimer.SetStartTime(AttackRate);
        AttackRateTimer.FuzzyStart();
        AttackRateTimer.Timeout += () =>
        {
            ShouldCloseAttackShapeCast.Enabled = true;
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleCloseMeleeAttackStart();
        HandleCloseMeleeAttackCollision();
    }

    public override void StartRagdoll()
    {
        base.StartRagdoll();
        AnimationPlayer.Free();
    }

    public void HandleCloseMeleeAttackStart()
    {
        if (!meleeAttacking && ShouldCloseAttackShapeCast.Enabled && ShouldCloseAttackShapeCast.IsColliding())
        {
            meleeAttackObjectsHit = [];
            meleeAttacking = true;
            Velocity = Vector3.Zero;
            NavigationAgent3D.Velocity = Vector3.Zero;
            CurrentNavComponent = AgentStopComponent;
        }
    }

    public void HandleCloseMeleeAttackCollision()
    {
        if (!CloseAttackRangeShapeCast.IsColliding()) return;
        for (int i = 0; i < CloseAttackRangeShapeCast.GetCollisionCount(); i++)
        {
            var collided = CloseAttackRangeShapeCast.GetCollider(i);
            if (collided is Hitbox hitbox && !meleeAttackObjectsHit.Contains(hitbox.Parent))
            {
                hitbox.Hit(BuildHitInformation(CloseAttackRangeShapeCast.GetCollisionPoint(i)));
                meleeAttackObjectsHit.Add(hitbox.Parent);
            }
        }
    }
}
