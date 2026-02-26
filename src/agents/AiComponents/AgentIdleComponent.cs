using Godot;
using System;
using FirstPerson.agents.AiComponents;

public partial class AgentIdleComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        if (!Grunt.IsOnFloor() && !Grunt.ShouldSnapToFloor())
        {
            Grunt.HandleFalling(delta);
            return;
        }
        Grunt.ApplyFloorSnap();

        if (Grunt.NavAgentMovementTargetNode is not null && Grunt.CanRotate())
        {
            Grunt.RotateToTarget();
        }
        
        if (NavigationAgent3D.AvoidanceEnabled)
        {
            NavigationAgent3D.Velocity = Vector3.Zero;
        }
        else
        {
            Grunt.OnVelocityComputed(Vector3.Zero);
        }
    }
}
