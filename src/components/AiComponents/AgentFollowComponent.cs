using Godot;
using System;
using FirstPerson.agents.AiComponents;

[GlobalClass]
public partial class AgentFollowComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }
        
        if (!Enemy.IsOnFloor())
        {
            Enemy.HandleFalling(delta);
            return;
        }

        if (Enemy.CombatTarget == null) return;
        
        NavigationAgent3D.TargetPosition =  Enemy.CanMove()
            ? Enemy.CombatTarget.GlobalPosition
            : Enemy.GlobalPosition;
        
        if (NavigationAgent3D.IsNavigationFinished())
        {
            Enemy.RotateToTarget();
            
            if (NavigationAgent3D.AvoidanceEnabled)
            {
                NavigationAgent3D.Velocity = Vector3.Zero;
            }
            else
            {
                Enemy.OnVelocityComputed(Vector3.Zero);
            }
            return;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - Enemy.GlobalPosition).Normalized();
        var currentVelocity = direction * Enemy.Speed;
        currentVelocity = currentVelocity with { Y = 0 };
        
        if (direction.Length() > 0.01f)
        {
            Enemy.RotateToGlobalPoint(direction);
        }
        else
        {
            Enemy.RotateToTarget();
        }

        if (NavigationAgent3D.AvoidanceEnabled)
        {
            NavigationAgent3D.Velocity = currentVelocity;
        }
        else
        {
            Enemy.OnVelocityComputed(currentVelocity);
        }
    }
}
