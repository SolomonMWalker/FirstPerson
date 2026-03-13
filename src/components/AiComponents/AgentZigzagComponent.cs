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
        
        if (!Grunt.IsOnFloor())
        {
            Grunt.HandleFalling(delta);
            return;
        }

        if (Grunt.CombatTarget == null) return;

        if (Grunt.GlobalPosition.DistanceTo(Grunt.CombatTarget.GlobalPosition) < _zigzagLength/5f)
        {
            AgentFollowComponent.HandleNavigation(delta);
            return;
        }

        if (!_zigzagTarget.HasValue || NavigationAgent3D.IsNavigationFinished())
        {
            SetZigZagTarget();
        }

        if (_zigzagTarget != null)
        {
            NavigationAgent3D.TargetPosition = Grunt.CanMove()
                ? _zigzagTarget.Value : Grunt.GlobalPosition;
        }

        var nextPathPosition = NavigationAgent3D.GetNextPathPosition();
        var direction = (nextPathPosition - Grunt.GlobalPosition).Normalized();
        var currentVelocity = direction * Grunt.Speed;
        currentVelocity = currentVelocity with { Y = 0 };
        
        if (direction.Length() > 0.01f)
        {
            Grunt.RotateToGlobalPoint(direction);
        }
        else
        {
            Grunt.RotateToTarget();
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

    public void SetZigZagTarget()
    {
        var dirToTargetV3 = (Grunt.GlobalPosition - Grunt.CombatTarget.GlobalPosition)
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
        dirToTargetV3 += Grunt.GlobalPosition;
        this.SpawnPermaMarker(dirToTargetV3);
        _zigzagRight = !_zigzagRight;
        if (_navMapId != null) _zigzagTarget = NavigationServer3D.MapGetClosestPoint(_navMapId.Value, dirToTargetV3);
    }
}
