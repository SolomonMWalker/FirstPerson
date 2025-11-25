using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ClamberController : Node3D
{
    [Export] public float width = 0;
    [Export] public float height = 0;
    [Export] public int numRaycastsWide = 3;
    [Export] public int numRaycastsHigh = 5;
    [Export] public float raycastLength = 0.1f;
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
        if (rawCollisions.Count(c => c.collided) == 1)
        {
            var collision = rawCollisions.First(c => c.collided);
            return (true, new RaycastCollisionResult
            {
                angleOfCollisions = null,
                globalPositionToClamberTo = collision.globalEndpoint
            });
        }

        var collisionsSorted = rawCollisions
            .OrderByDescending(c => c.localSlice.Y)
            .ToList();
        //if top raycast didn't collide, try to mantle to top spot found
        if (!collisionsSorted[0].collided)
        {
            var globalEndpoint = collisionsSorted
                .First(c => c.collided)
                .globalEndpoint;
            
            return (true, new RaycastCollisionResult
            {
                angleOfCollisions = null,
                globalPositionToClamberTo = globalEndpoint
            });
        }
        //find angle between top 2 collisions, if acceptable, mantle to top
        var top2 = collisionsSorted.Where(c => c.collided).Take(2).ToList();
        var top = top2[0];
        var next = top2[1];
        //top is past next z, then we have an upside down wedge, can't climb that
        if (top.localSlice.X <= next.localSlice.X) return (false, null);
        var angleToInRads = next.localSlice.AngleTo(top.localSlice);
        var angleToInDeg = Mathf.RadToDeg(angleToInRads);
        if (angleToInDeg > maxAngleInDeg)
        {
            return (false, null);
        }

        return (true, new RaycastCollisionResult
        {
            angleOfCollisions = angleToInDeg,
            globalPositionToClamberTo = top.globalEndpoint
        });
    }

}

public class RaycastCollisionResult
{
    public float? angleOfCollisions = null;
    public Vector3? globalPositionToClamberTo;
}
