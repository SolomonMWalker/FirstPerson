using Godot;
using System;
using FirstPerson.agents.AiComponents;

public partial class AgentFollowComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }
        
        NavigationAgent3D.TargetPosition = Grunt.NavAgentMovementTargetNode.GlobalPosition;
        
        if (!Grunt.IsOnFloor())
        {
            Grunt.HandleFalling(delta);
            return;
        }

        if (Grunt.NavAgentMovementTargetNode == null) return;
        
        if (NavigationAgent3D.IsNavigationFinished() || !Grunt.CanMove())
        {
            if (Grunt.CanRotate())
            {
                Grunt.RotateToTarget();
            }
            return;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - Grunt.GlobalPosition).Normalized();
        var currentVelocity = direction * Grunt.Speed;
        currentVelocity = currentVelocity with { Y = 0 };
        
        if (direction.Length() > 0.01f && Grunt.CanRotate())
        {
            Grunt.RotateToGlobalPoint(direction);
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
