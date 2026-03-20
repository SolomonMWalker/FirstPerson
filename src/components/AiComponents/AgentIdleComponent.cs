using Godot;
using System;
using FirstPerson.agents.AiComponents;

[GlobalClass]
public partial class AgentIdleComponent : BaseAiNavComponent
{
    public override void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }
        
        Enemy.NavigationAgent3D.TargetPosition = Enemy.GlobalPosition;
        
        if (!Enemy.IsOnFloor())
        {
            Enemy.HandleFalling(delta);
            return;
        }

        if (Enemy.CombatTarget is not null && Enemy.CanRotate())
        {
            Enemy.RotateToTarget();
        }
    }
}
