using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ClamberController : Node3D
{
    [Export] public float raycastLength = 0.25f;
    [Export] public float clamberMargin = 0.26f;
    [Export] public float maxAngleInDeg = 10f;
    [Export] public float waitPerCallInSec = 0.25f;

    private double _timeSinceLastClamberCall = 0;
    private List<List<RayCast3D>> _raycasts = [];
    private Node3D _raycastsParent;
    private Vector2 _topLeftCorner;

    public override void _Ready()
    {
        base._Ready();
        _raycastsParent = GetNode<Node3D>("Raycasts");
        foreach (var child in _raycastsParent.GetChildren())
        {
            List<RayCast3D> rcList = [];
            rcList.AddRange(child.GetChildren().Cast<RayCast3D>());
            _raycasts.Add(rcList);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _timeSinceLastClamberCall += delta;
        _timeSinceLastClamberCall = Mathf.Clamp(_timeSinceLastClamberCall, 0, waitPerCallInSec);
    }

    private List<(Vector2 localSlice, Vector3 globalEndpoint, bool collided)> GetRaycastEndPoints(List<RayCast3D> raycasts)
    {
        List<(Vector2, Vector3, bool)> collisions = [];
        foreach (var rc in raycasts)
        {
            //switch this shit to 2d with global z and y
            if (rc.IsColliding())
            {
                var globalEndpoint = rc.GetCollisionPoint();
                var endpoint = ToLocal(globalEndpoint);
                collisions.Add((new Vector2(endpoint.Z, endpoint.Y), globalEndpoint, true));
            }
            else
            {
                var globalEndpoint = rc.ToGlobal(rc.TargetPosition);
                var endpointLocalToThis = ToLocal(globalEndpoint);
                collisions.Add((new Vector2(endpointLocalToThis.Z, endpointLocalToThis.Y), globalEndpoint, false));
            }
        }
        return collisions;
    }
    
    private (bool success, RaycastCollisionResult result) AttemptClamberCheckRow(List<RayCast3D> raycasts)
    {
        var rawCollisions = GetRaycastEndPoints(raycasts);
        if (rawCollisions.All(c => !c.collided)) return (false, null);

        //If top raycast is colliding, we can't clamber
        var maxY = rawCollisions.Select(rc => rc.localSlice.Y).Max();
        var collidedCollisions = rawCollisions
            .Where(rc => rc.collided)
            .ToArray();
        if (collidedCollisions.Any(rc => Math.Abs(rc.localSlice.Y - maxY) < 0.0001f))
        {
            return (false, null);
        }

        var clamberPoint = collidedCollisions
            .OrderByDescending(c => c.localSlice.Y)
            .First();
        return (true, new RaycastCollisionResult
        {
            globalPositionToClamberTo = clamberPoint.globalEndpoint
        });
        //took out extra "check for angle" code for now
    }

    public (bool success, RaycastCollisionResult result) AttemptClamber()
    {
        if (_timeSinceLastClamberCall < waitPerCallInSec) return (false, null);
        _timeSinceLastClamberCall = 0;
        foreach (var rcList in _raycasts)
        {
            var clamberAttemptRow = AttemptClamberCheckRow(rcList);
            if (clamberAttemptRow.success) return clamberAttemptRow;
        }

        return (false, null);
    }
}

public class RaycastCollisionResult
{
    public Vector3? globalPositionToClamberTo;
}
