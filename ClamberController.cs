using Godot;
using System;
using System.Collections.Generic;

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
                    _topLeftCorner.X + (i/(float)numRaycastsWide)*width,
                    _topLeftCorner.Y - (i/(float)numRaycastsHigh)*height,
                    0f);
                ray.TargetPosition = ray.Position + new Vector3(0, 0, raycastLength);
                _raycasts.Add(ray);
                _raycastsParent.AddChild(ray);
            }
        }
    }
}
