using Godot;
using System;

[Tool]
public partial class MovingPlatform : AnimatableBody3D
{
    [Export] public float MoveDistance { get; set; } = 2.0f;
    [Export] public float MoveTime { get; set; } = 2.0f;
    [Export] public Vector3 MoveDirection { get; set; } = new (0, 1, 0);

    public Vector3 startPosition, endPosition;
    public Tween platformTween;

    public void FuncGodotApplyProperties(Godot.Collections.Dictionary<string, Variant> entityProperties)
    {
        MoveDistance = (float) entityProperties["MoveDistance"];
        MoveTime = (float) entityProperties["MoveTime"];
        MoveDirection = (Vector3) entityProperties["MoveDirection"];
    }

    public override void _Ready()
    {
        base._Ready();
        //if(!Engine.IsEditorHint()) https://youtu.be/kIvK4rzbqUs?si=OZn4afmICvkkSuPk&t=59
    }
}
