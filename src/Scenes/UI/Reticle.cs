using Godot;
using System;

public partial class Reticle : Container
{
    [Export] public float DotRadius { get; private set; } = 1.0f;
    [Export] public Color DotColor { get; private set; } = Colors.White;

    public override void _Draw()
    {
        base._Draw();
        DrawCircle(Vector2.Zero, DotRadius, DotColor);
    }
}
