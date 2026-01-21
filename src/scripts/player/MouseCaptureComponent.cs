using Godot;
using System;

public partial class MouseCaptureComponent : Node
{
    [Export] public bool Debug { get; set; }

    [ExportCategory("MouseCaptureSettings")]
    [Export] public Input.MouseModeEnum CurrentMouseMode { get; set; } = Input.MouseModeEnum.Captured;

    [Export] public float MouseSensitivity { get; set; } = 0.005f; 
    
    public Vector2 MouseInput { get; private set; }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (Input.MouseMode == Input.MouseModeEnum.Captured && @event is InputEventMouseMotion mouseMotionEvent)
        {
            MouseInput = MouseInput with
            {
                X = MouseInput.X + -mouseMotionEvent.ScreenRelative.X * MouseSensitivity,
                Y = MouseInput.Y + -mouseMotionEvent.ScreenRelative.Y * MouseSensitivity
            };
        }
    }

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = CurrentMouseMode;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MouseInput = Vector2.Zero;
    }
}
