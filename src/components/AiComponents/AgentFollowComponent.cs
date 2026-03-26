using Godot;
using System;
using FirstPerson.agents.AiComponents;

[GlobalClass]
public partial class AgentFollowComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        GD.Print("following");
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            GD.Print("nav map shenanigans");
            return;
        }
        
        if (!MovingAgent.IsOnFloor())
        {
            MovingAgent.HandleFalling(delta);
            return;
        }

        if (MovingAgent.CombatTarget == null) return;
        
        NavigationAgent3D.TargetPosition =  MovingAgent.CanMove()
            ? MovingAgent.CombatTarget.GlobalPosition
            : MovingAgent.GlobalPosition;
        
        if (NavigationAgent3D.IsNavigationFinished())
        {
            MovingAgent.RotateToTarget();
            
            if (NavigationAgent3D.AvoidanceEnabled)
            {
                NavigationAgent3D.Velocity = Vector3.Zero;
            }
            else
            {
                MovingAgent.OnVelocityComputed(Vector3.Zero);
            }
            return;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - MovingAgent.GlobalPosition).Normalized();
        var currentVelocity = direction * MovingAgent.Speed;
        currentVelocity = currentVelocity with { Y = 0 };
        GD.Print($"set velocity to {currentVelocity}");
        
        if (direction.Length() > 0.01f)
        {
            MovingAgent.RotateToGlobalPoint(direction);
        }
        else
        {
            MovingAgent.RotateToTarget();
        }

        if (NavigationAgent3D.AvoidanceEnabled)
        {
            NavigationAgent3D.Velocity = currentVelocity;
        }
        else
        {
            MovingAgent.OnVelocityComputed(currentVelocity);
        }
    }
}
