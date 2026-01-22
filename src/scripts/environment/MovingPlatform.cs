using Godot;

[Tool]
public partial class MovingPlatform : AnimatableBody3D
{
    [Export] public float MoveDistance { get; set; }
    [Export] public float MoveTime { get; set; }
    [Export] public Vector3 MoveDirection { get; set; }

    public Vector3 startPosition, endPosition;
    public Tween platformTween;

    public void _func_godot_apply_properties(Godot.Collections.Dictionary<string, Variant> entityProperties)
    {
        MoveDistance = (float) entityProperties["MoveDistance"];
        MoveTime = (float) entityProperties["MoveTime"];
        MoveDirection = (Vector3) entityProperties["MoveDirection"];
    }

    public override void _Ready()
    {
        base._Ready();
        if (!Engine.IsEditorHint())
        {
            startPosition = GlobalPosition;
            endPosition = startPosition + (MoveDirection.Normalized()) * MoveDistance;
            StartMovement();
        }
    }

    public void StartMovement()
    {
        platformTween = CreateTween();
        platformTween.SetLoops(20);
        GD.Print($"Start position is {startPosition}");
        GD.Print($"End position is {endPosition}");
        platformTween.TweenProperty(this, "global_position", endPosition, MoveTime);
        platformTween.TweenProperty(this, "global_position", startPosition, MoveTime);
    }
}
