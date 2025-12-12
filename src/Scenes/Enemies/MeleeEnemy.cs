using Godot;
using System;
using FirstPerson;

public partial class MeleeEnemy : Agent
{
    protected ShapeCast3D MeleeRangeShapeCast { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        MeleeRangeShapeCast = GetNode<ShapeCast3D>("MeleeRangeShapeCast");
        AllowedGoals.Add(Goal.MoveToCover);
        CurrentGoal = Goal.MoveToCover;
        
        Target = GetNode<HittableCharacterBody3D>("/root/Test/EnemyTarget");
        //Target = GetNode<ShootableCharacterBody3D>(Configuration.GetConfigValues().PlayerSceneTreePath);
    }

    protected override void CalculateNavigation(double delta)
    {
        switch (CurrentGoal)
        {
            case Goal.MoveToCover:
                MoveToCover(delta);
                break;
            case Goal.MoveToTargetMedium:
                MoveToTarget();
                break;
        }
    }

    protected override void MoveToTarget()
    {
        SetNavigationToTarget();
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
