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

    private List<RayCast3D> _raycasts = [];
    private Node3D _raycastsParent;
    private Vector2 _topLeftCorner;

    public override void _Ready()
    {
        base._Ready();
        _raycastsParent = GetNode<Node3D>("Raycasts");
        _topLeftCorner = new Vector2(-(width / 2), height / 2);
        BuildRaycasts();
        GD.Print($"Built raycasts with {_raycasts.Count} rays");
    }


    private void BuildRaycasts()
    {
        for (int i = 0; i < numRaycastsWide; i++)
        {
            for (int j = numRaycastsHigh; j > 0; j--)
            {
                var ray = new RayCast3D();
                ray.Position = new Vector3(
                    _topLeftCorner.X + (i / (float)numRaycastsWide) * width,
                    _topLeftCorner.Y - (i / (float)numRaycastsHigh) * height,
                    0f);
                ray.TargetPosition = ray.Position + new Vector3(0, 0, raycastLength);
                _raycasts.Add(ray);
                _raycastsParent.AddChild(ray);
            }
        }
    }

    public List<Vector3> GetRaycastCollisions()
    {
        List<Vector3> collisions = [];
        foreach (var rc in _raycasts)
        {
            if (!rc.CollideWithBodies) continue;
            collisions.Add(rc.GetCollisionPoint());
        }

        return collisions;
    }

    public (float w, float h) DistanceBetweenRaycasts()
    {
        return (width / numRaycastsWide, height / numRaycastsHigh);
    }

    public RaycastCollisionResult GetRaycastCollisionResult()
    {
        var collisions = GetRaycastCollisions().Select(c => c.Y).Distinct().ToList();
        if (collisions.Count == 0) return new RaycastCollisionResult();
        if (collisions.Count == 1)
        {
            return new RaycastCollisionResult
            {
                angleOfCollisions = null,
                heightToRise = collisions[0] + height / numRaycastsHigh
            };
        }

        var top2Collisions = collisions.OrderByDescending(c => c).Take(2).ToList();
        //get angle of top 2 collisions to see if we can clamber
    }

}

public class RaycastCollisionResult
{
    public float? angleOfCollisions;
    public float heightToRise;
}
