using Godot;
using System;
using FirstPerson;
using FirstPerson.Configuration;
using FirstPerson.Helpers;

public partial class MeleeEnemy : Agent
{
    public float CloseFollowDistance { get; protected set; } = 1f;
    public float PauseTimeBetweenAttacks { get; protected set; } = 1f;
    protected ShapeCast3D MeleeRangeShapeCast { get; set; }
    protected Area3D MeleeRangeArea { get; set; }
    protected Poll PauseBetweenAttacksPoll { get; set; }
    protected bool IsAttacking { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        CurrentFollowDistance = CloseFollowDistance;
        PauseBetweenAttacksPoll = new Poll(PauseTimeBetweenAttacks);
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
        if (IsAttacking && (!AnimationPlayer.IsPlaying() || AnimationPlayer.CurrentAnimation != "MeleeAttack" ))
        {
            IsAttacking = false;
            PauseBetweenAttacksPoll.ResetPoll();
        }
        else
        {
            PauseBetweenAttacksPoll.AdvanceTimeWithoutPing(delta);
        }
        
        if (!MeleeRangeArea.HasOverlappingBodies() || 
            (AnimationPlayer.IsPlaying() && AnimationPlayer.CurrentAnimation == "MeleeAttack")) return;
        if (!PauseBetweenAttacksPoll.IsPollPinged(delta)) return;
        AnimationPlayer.Play("MeleeAttack");
        IsAttacking = true;
    }
}
