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
        
        if (!Grunt.IsOnFloor())
        {
            Grunt.HandleFalling(delta);
            return;
        }

        if (Grunt.CombatTarget == null) return;
        
        NavigationAgent3D.TargetPosition =  Grunt.CanMove()
            ? Grunt.CombatTarget.GlobalPosition
            : Grunt.GlobalPosition;
        
        if (NavigationAgent3D.IsNavigationFinished())
        {
            Grunt.RotateToTarget();
            
            if (NavigationAgent3D.AvoidanceEnabled)
            {
                NavigationAgent3D.Velocity = Vector3.Zero;
            }
            else
            {
                Grunt.OnVelocityComputed(Vector3.Zero);
            }
            return;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - Grunt.GlobalPosition).Normalized();
        var currentVelocity = direction * Grunt.Speed;
        currentVelocity = currentVelocity with { Y = 0 };
        
        if (direction.Length() > 0.01f)
        {
            Grunt.RotateToGlobalPoint(direction);
        }
        else
        {
            Grunt.RotateToTarget();
        }

        if (NavigationAgent3D.AvoidanceEnabled)
        {
            NavigationAgent3D.Velocity = currentVelocity;
        }
        else
        {
            Grunt.OnVelocityComputed(currentVelocity);
        }
    }
}
