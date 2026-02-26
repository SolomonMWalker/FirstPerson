using Godot;
using System;
using FirstPerson.agents.AiComponents;

public partial class AgentIdleComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        var velocityNoXz = Grunt.CharacterBody3D.Velocity with { X = 0, Z = 0 };
        var currentVelocity = Grunt.AddGravityToVelocity(velocityNoXz, delta);

        if (Grunt.NavAgentMovementTargetNode is not null)
        {
            Grunt.RotateToTarget();
        }
        
        if (NavigationAgent3D.AvoidanceEnabled)
        {
            NavigationAgent3D.Velocity = currentVelocity;
        }
        Grunt.OnVelocityComputed(currentVelocity);
    }
}
