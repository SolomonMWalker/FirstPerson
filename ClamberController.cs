using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ClamberController : Node3D
{
    [Export] public float width = 0;
    [Export] public float height = 0;
    [Export] public int numRaycastsWide = 3;
    [Export] public int numRaycastsHigh = 5;
    [Export] public float raycastLength = 0.25f;
    [Export] public float clamberMargin = 0.26f;
    [Export] public float maxAngleInDeg = 10f;
    [Export] public float waitPerCallInSec = 0.15f;

    private double _timeSinceLastClamberCall = 0;
    private RayCast3D _topRaycast;
    private List<RayCast3D> _raycasts = [];
    private Node3D _raycastsParent;
    private Vector2 _topLeftCorner;

    public override void _Ready()
    {
        base._Ready();
        _raycastsParent = GetNode<Node3D>("Raycasts");
        foreach (var child in _raycastsParent.GetChildren())
        {
            _raycasts.Add((RayCast3D)child);
        }

        _topRaycast = _raycasts.OrderByDescending(rc => rc.GlobalPosition.Y).First();
        GD.Print($"Built raycasts with {_raycasts.Count} rays");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _timeSinceLastClamberCall += delta;
        _timeSinceLastClamberCall = Mathf.Clamp(_timeSinceLastClamberCall, 0, waitPerCallInSec);
    }


    private void BuildRaycasts()
    {
        for (int i = 0; i < numRaycastsWide; i++)
        {
            for (int j = numRaycastsHigh; j > 0; j--)
            {
                var ray = new RayCast3D();
                ray.Enabled = true;
                ray.Position = new Vector3(
                    _topLeftCorner.X + (i / (float)numRaycastsWide) * width,
                    _topLeftCorner.Y - (i / (float)numRaycastsHigh) * height,
                    0f);
                ray.TargetPosition = ray.Position + new Vector3(0, 0, -raycastLength);
                _raycasts.Add(ray);
                _raycastsParent.AddChild(ray);
            }
        }
    }

    public List<(Vector2 localSlice, Vector3 globalEndpoint, bool collided)> GetRaycastEndPoints()
    {
        List<(Vector2, Vector3, bool)> collisions = [];
        foreach (var rc in _raycasts)
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

        /*foreach (var c in collisions)
        {
            GD.Print($"collision {c.Item1} globalCollision {c.Item2} collided {c.Item3}");
        }*/
        
        return collisions;
    }
    

    public (bool success, RaycastCollisionResult result) AttemptClamber()
    {
        if (_timeSinceLastClamberCall < waitPerCallInSec) return (false, null);
        _timeSinceLastClamberCall = 0;
        var rawCollisions = GetRaycastEndPoints();
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

}

public class RaycastCollisionResult
{
    public Vector3? globalPositionToClamberTo;
}
