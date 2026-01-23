using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FirstPerson.Helpers;

public partial class ClamberController : Node3D
{
    [Export] public Timer PauseBetweenClamberAttemptsTimer { get; set; }
    [Export] public float RaycastLength { get; private set; } = 0.25f;
    [Export] public float ClamberMargin { get; private set; } = 0.26f;
    //[Export] public float MaxAngleInDeg { get; private set; } = 10f;
    [Export] public float WaitPerCallInSec { get; private set; } = 0.25f;

    private double TimeSinceLastClamberCall { get; set; }
    private List<List<RayCast3D>> Raycasts { get; set; } = [];
    private Node3D RaycastsParent { get; set; }
    private Vector2 TopLeftCorner { get; set; }

    public override void _Ready()
    {
        base._Ready();
        RaycastsParent = GetNode<Node3D>("Raycasts");
        foreach (var child in RaycastsParent.GetChildren())
        {
            List<RayCast3D> rcList = [];
            rcList.AddRange(child.GetChildren().Cast<RayCast3D>());
            Raycasts.Add(rcList);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        TimeSinceLastClamberCall += delta;
        TimeSinceLastClamberCall = Mathf.Clamp(TimeSinceLastClamberCall, 0, WaitPerCallInSec);
    }

    private List<(Vector2 localSlice, Vector3 globalEndpoint, bool collided)> GetRaycastEndPoints(List<RayCast3D> raycasts)
    {
        List<(Vector2, Vector3, bool)> collisions = [];
        foreach (var rc in raycasts)
        {
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
            GlobalPositionToClamberTo = clamberPoint.globalEndpoint
        });
        //took out extra "check for angle" code for now
    }

    public (bool success, RaycastCollisionResult result) AttemptClamber()
    {
        if (!PauseBetweenClamberAttemptsTimer.IsStopped()) return (false, null);
        PauseBetweenClamberAttemptsTimer.Start();
        foreach (var rcList in Raycasts)
        {
            var clamberAttemptRow = AttemptClamberCheckRow(rcList);
            if (clamberAttemptRow.success) return clamberAttemptRow;
        }
        return (false, null);
    }
}

public class RaycastCollisionResult
{
    public Vector3? GlobalPositionToClamberTo { get; init; }
}