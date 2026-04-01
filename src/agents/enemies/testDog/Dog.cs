using Godot;
using System;

public partial class Dog : CombatAgent
{
    [ExportCategory("Dog Settings")]
    [Export] public float AttackRate { get; set; } = 2.0f;
    
    [ExportCategory("Components")]
    [Export] public AgentZigzagComponent AgentZigzagComponent { get; set; }
    [Export] public AgentFollowComponent AgentFollowComponent { get; set; }
    [Export] public AgentIdleComponent AgentIdleComponent { get; set; }
    
    [ExportCategory("References")]
    [Export] public CustomAnimationTree CustomAnimationTree { get; set; }
    [Export] public AnimationPlayer AnimationPlayer { get; set; }
    [Export] public ShapeCast3D ShouldCloseAttackShapeCast { get; set; }
    [Export] public ShapeCast3D CloseAttackRangeShapeCast { get; set; }
    [Export] public FuzzyStartTimer AttackRateTimer { get; set; }

    public bool meleeAttacking, leapAttacking, nextAttackIsLeap;
    public bool Attacking { get => _attacking; }
    private bool _attacking;
    public override bool TurnOffAi() => base.TurnOffAi() && _attacking;
    public override bool CanRotate() => base.CanRotate() && !_attacking;
    public void StartAttacking() => _attacking = true;
    public void StopAttacking() => _attacking = false;
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
        CurrentNavComponent = AgentIdleComponent;
        foreach (var hitbox in Hitboxes)
        {
            CloseAttackRangeShapeCast.AddException(hitbox);
            ShouldCloseAttackShapeCast.AddException(hitbox);
        }
        AttackRateTimer.SetStartTime(AttackRate);
        AttackRateTimer.FuzzyStart();
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
        if (!meleeAttacking && ShouldCloseAttackShapeCast.IsColliding())
        {
            GD.Print("melee attacking");
            meleeAttacking = true;
            Velocity = Vector3.Zero;
            NavigationAgent3D.Velocity = Vector3.Zero;
        }
    }

    public void HandleCloseMeleeAttackCollision()
    {
        if (!CloseAttackRangeShapeCast.IsColliding()) return;
        GD.Print("attack hit");
        for (int i = 0; i < CloseAttackRangeShapeCast.GetCollisionCount(); i++)
        {
            var collided = CloseAttackRangeShapeCast.GetCollider(i);
            if (collided is Hitbox hitbox)
            {
                hitbox.Hit(BuildHitInformation(CloseAttackRangeShapeCast.GetCollisionPoint(i)));
                CloseAttackRangeShapeCast.AddException(hitbox);
            }
        }
    }
}
