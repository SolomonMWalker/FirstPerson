using Godot;
using System;

public partial class WeaponSubViewport : SubViewport
{
    private Vector2I ScreenSize { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
        ScreenSize = GetWindow().Size;
        Size = ScreenSize;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (GetWindow().Size != ScreenSize)
        {
            ScreenSize = Size;
            Size = ScreenSize;
        }
    }
}
