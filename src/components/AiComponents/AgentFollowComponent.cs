using Godot;
using System;
using FirstPerson.agents.AiComponents;

[GlobalClass]
public partial class AgentFollowComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (!HasNavMap())
        {
            return;
        }
        
        if (!MovingAgent.IsOnFloor())
        {
            MovingAgent.HandleFalling(delta);
            return;
        }

        if (MovingAgent.MovementTarget == null) return;
        
        MovingAgent.MoveToPoint(delta, MovingAgent.MovementTarget.GlobalPosition, MovingAgent.Speed);
    }
}
