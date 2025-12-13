using Godot;
using System;
using FirstPerson;

public partial class MeleeEnemy : Agent
{
    public float CloseFollowDistance { get; protected set; } = 1f;
    protected ShapeCast3D MeleeRangeShapeCast { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        CurrentFollowDistance = CloseFollowDistance;
        
        MeleeRangeShapeCast = GetNode<ShapeCast3D>("MeleeRangeShapeCast");
        AllowedGoals.Add(Goal.MoveToTargetClose);
        CurrentGoal = Goal.MoveToTargetClose;
        
        Target = GetNode<HittableCharacterBody3D>("/root/Test/EnemyTarget");
        //Target = GetNode<ShootableCharacterBody3D>(Configuration.GetConfigValues().PlayerSceneTreePath);
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
}
