using Godot;
using System;
using FirstPerson.agents.AiComponents;

public partial class AgentIdleComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }
        
        Grunt.NavigationAgent3D.TargetPosition = Grunt.GlobalPosition;
        
        if (!Grunt.IsOnFloor())
        {
            Grunt.HandleFalling(delta);
            return;
        }

        if (Grunt.NavAgentMovementTargetNode is not null && Grunt.CanRotate())
        {
            Grunt.RotateToTarget();
        }
    }
}
