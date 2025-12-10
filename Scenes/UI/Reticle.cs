using Godot;
using System;

public partial class Reticle : Container
{
    [Export] public float dotRadius = 1.0f;
    [Export] public Color dotColor = Colors.White;

    public override void _Draw()
    {
        base._Draw();
        DrawCircle(Vector2.Zero, dotRadius, dotColor);
    }
}
