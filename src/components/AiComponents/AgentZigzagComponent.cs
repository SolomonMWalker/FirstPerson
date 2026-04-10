using Godot;
using System;
using FirstPerson.agents.AiComponents;
using FirstPerson.Helpers;
using Godot.NativeInterop;

[GlobalClass]
public partial class AgentZigzagComponent : BaseAiNavComponent
{
    [Export] public AgentFollowComponent AgentFollowComponent;
    
    private bool _doneWithFirstZigzag;
    private bool _zigzagRight;
    private float _zigzagLength = 15f;
    private float _zigzagAngleInDeg = 20f;
    private float _zigzagStopLength = 5f;
    private float _zigzagAngleInRad;
    private Vector3? _zigzagTarget;
    private Rid? _navMapId;

    public override void _Ready()
    {
        base._Ready();
        _zigzagAngleInRad = Mathf.DegToRad(_zigzagAngleInDeg);
    }

    public override void HandleNavigation(double delta)
    {
        // Do not query when the map has never synchronized and is empty.
        if (NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) == 0)
        {
            return;
        }
        _navMapId ??= NavigationAgent3D.GetNavigationMap();
        
        if (!MovingAgent.IsOnFloor())
        {
            MovingAgent.HandleFalling(delta);
            return;
        }

        if (MovingAgent.MovementTarget == null) return;

        if (MovingAgent.GlobalPosition.DistanceTo(MovingAgent.MovementTarget.GlobalPosition) < _zigzagStopLength)
        {
            AgentFollowComponent.HandleNavigation(delta);
            return;
        }

        if (!_zigzagTarget.HasValue || NavigationAgent3D.IsNavigationFinished())
        {
            SetZigZagTarget();
        }

        var targetPosition = MovingAgent.GlobalPosition;
        
        if (_zigzagTarget != null && MovingAgent.CanMove())
        {
            targetPosition = _zigzagTarget.Value;
        }

        MovingAgent.MoveToPoint(delta, targetPosition, MovingAgent.Speed);
    }

    public void SetZigZagTarget()
    {
        var dirToTargetV3 = (MovingAgent.GlobalPosition - MovingAgent.MovementTarget.GlobalPosition)
            .Rotated(Vector3.Up, Mathf.DegToRad(180))
            .Normalized();
        if (!_doneWithFirstZigzag)
        {
            dirToTargetV3 *= _zigzagLength / 2f;
        }
        else
        {
            dirToTargetV3 *= _zigzagLength;
            _doneWithFirstZigzag = true;
        }

        dirToTargetV3 = dirToTargetV3.Rotated(Vector3.Up, _zigzagRight ? _zigzagAngleInRad : -_zigzagAngleInRad);
        dirToTargetV3 += MovingAgent.GlobalPosition;
        _zigzagRight = !_zigzagRight;
        if (_navMapId != null) _zigzagTarget = NavigationServer3D.MapGetClosestPoint(_navMapId.Value, dirToTargetV3);
    }
}
