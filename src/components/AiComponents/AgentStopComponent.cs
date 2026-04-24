using Godot;
using System;
using FirstPerson.agents.AiComponents;

[GlobalClass]
public partial class AgentStopComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (!HasNavMap())
        {
            return;
        }
        
        MovingAgent.NavigationAgent3D.TargetPosition = MovingAgent.GlobalPosition;
        
        if (!MovingAgent.IsOnFloor())
        {
            MovingAgent.HandleFalling(delta);
            return;
        }

        if (MovingAgent.MovementTarget is not null && MovingAgent.CanRotate())
        {
            MovingAgent.RotateToTarget();
        }
    }
}
