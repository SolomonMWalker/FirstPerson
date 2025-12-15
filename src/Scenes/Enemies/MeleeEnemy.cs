using Godot;
using System;
using FirstPerson;
using FirstPerson.Configuration;

public partial class MeleeEnemy : Agent
{
    public float CloseFollowDistance { get; protected set; } = 1f;
    public float PauseTimeBetweenAttacks { get; protected set; } = 2f;
    protected ShapeCast3D MeleeRangeShapeCast { get; set; }
    protected Area3D MeleeRangeArea { get; set; }
    protected bool IsAttacking { get; set; }
    protected double TimeSinceLastAttackEnded { get; set; } = double.MaxValue;
    
    public override void _Ready()
    {
        base._Ready();
        CurrentFollowDistance = CloseFollowDistance;
        FreezeMotionBools.Add(IsAttacking);
        
        MeleeRangeShapeCast = GetNode<ShapeCast3D>("MeleeRangeShapeCast");
        MeleeRangeArea = GetNode<Area3D>("MeleeRangeArea");
        AllowedGoals.Add(Goal.MoveToTargetClose);
        CurrentGoal = Goal.MoveToTargetClose;
        
        //Target = GetNode<HittableCharacterBody3D>("/root/Test/EnemyTarget");
        Target = GetNode<HittableCharacterBody3D>(Configuration.GetConfigValues().PlayerSceneTreePath);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        HitCollidersInCast();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleAttack(delta);
    }

    protected override bool IsMotionFrozen()
    {
        return IsAttacking || base.IsMotionFrozen();
    }

    protected override void CalculateNavigation(double delta)
    {
        switch (CurrentGoal)
        {
            case Goal.MoveToTargetClose:
                MoveToTarget(delta);
                break;
        }
    }

    protected virtual void HitCollidersInCast()
    {
        if (!MeleeRangeShapeCast.IsColliding()) return;
        for (int i = 0; i < MeleeRangeShapeCast.GetCollisionCount(); i++)
        {
            var collidedObject = MeleeRangeShapeCast.GetCollider(i);
            if (collidedObject is HittableCharacterBody3D hittableChar)
            {
                hittableChar.Hit(new HitParameters(8));
            }
        }
    }

    protected virtual void HandleAttack(double delta)
    {
        if (IsAttacking && !AnimationPlayer.IsPlaying())
        {
            IsAttacking = false;
            //GD.Print("IsAttacking = false");
            TimeSinceLastAttackEnded = 0;
        }
        else
        {
            TimeSinceLastAttackEnded += delta;
        }
        
        if (!(TimeSinceLastAttackEnded > PauseTimeBetweenAttacks)) return;
        if (!MeleeRangeArea.HasOverlappingBodies() || AnimationPlayer.IsPlaying()) return;
        AnimationPlayer.Play("MeleeAttack");
        IsAttacking = true;
        //GD.Print("IsAttacking = true");
    }
}
