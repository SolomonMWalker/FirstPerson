using Godot;

namespace FirstPerson.agents.AiComponents;

public partial class AgentPatrolComponent : BaseAiNavComponent
{
    [Export] public Godot.Collections.Array<Node3D> PatrolPoints;
    
    private int _currentPatrolPointIndex;

    public bool HasPatrolPoints() => PatrolPoints.Count > 0;
    public override void HandleNavigation(double delta)
    {
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

        if (PatrolPoints.Count < 1)
        {
            MovingAgent.RotateToTarget();
            return;
        }

        if (NavigationAgent3D.IsNavigationFinished() && PatrolPoints.Count > 1)
        {
            _currentPatrolPointIndex += 1;
            _currentPatrolPointIndex %= PatrolPoints.Count;
        }
        
        MovingAgent.MoveToPoint(delta, PatrolPoints[_currentPatrolPointIndex].GlobalPosition, MovingAgent.Speed);
    }
}