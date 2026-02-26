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

        if (Grunt.NavAgentMovementTargetNode == null) return;
        
        NavigationAgent3D.TargetPosition = Grunt.NavAgentMovementTargetNode.GlobalPosition;
        
        if (NavigationAgent3D.IsNavigationFinished())
        {
            var velocityNoXz = Grunt.CharacterBody3D.Velocity with { X = 0, Z = 0 };
            var gravOnlyVelocity = Grunt.AddGravityToVelocity(velocityNoXz, delta);
            if (!Grunt.freezeRotation)
            {
                Grunt.RotateToTarget();
            }
            Grunt.OnVelocityComputed(gravOnlyVelocity);
            return;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - Grunt.CharacterBody3D.GlobalPosition).Normalized();
        var currentVelocity = Grunt.AddGravityToVelocity(direction * Grunt.Speed, delta);
        
        if (direction.Length() > 0.01f || Grunt.freezeRotation)
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
