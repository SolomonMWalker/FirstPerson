using Godot;
using System;

public partial class MouseCaptureComponent : Node
{
    [Export] public bool Debug { get; set; }

    [ExportCategory("MouseCaptureSettings")]
    [Export] public Input.MouseModeEnum CurrentMouseMode { get; set; } = Input.MouseModeEnum.Captured;

    [Export] public float MouseSensitivity { get; set; } = 0.005f; 
    
    public Vector2 RelativeMouseInputWithSens { get; private set; }
    public Vector2 RawRelativeMouseInput { get; private set; }
    
    
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (Input.MouseMode == Input.MouseModeEnum.Captured && @event is InputEventMouseMotion mouseMotionEvent)
        {
            RelativeMouseInputWithSens = RelativeMouseInputWithSens with
            {
                X = RelativeMouseInputWithSens.X + -mouseMotionEvent.ScreenRelative.X * MouseSensitivity,
                Y = RelativeMouseInputWithSens.Y + -mouseMotionEvent.ScreenRelative.Y * MouseSensitivity
            };

            RawRelativeMouseInput += mouseMotionEvent.Relative;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = CurrentMouseMode;
        ProcessPriority = 1;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        RelativeMouseInputWithSens = Vector2.Zero;
        RawRelativeMouseInput = Vector2.Zero;
    }
}
