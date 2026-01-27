using System.Linq;
using FirstPerson.Scenes.Player;
using Godot;

public partial class Reticle : CenterContainer
{
    [Export] public PlayerController Player { get; set; }
    [Export] public Godot.Collections.Array<Line2D> ReticleLines { get; set; }
    [Export] public float ReticleSpeed { get; set; } = 0.25f;
    [Export] public float ReticleDistance { get; set; } = 2f;
    [Export] public float DotRadius { get; private set; } = 1.0f;
    [Export] public Color DotColor { get; private set; } = Colors.White;

    public override void _Ready()
    {
        base._Ready();
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        AdjustReticleLines();
    }

    public override void _Draw()
    {
        base._Draw();
        DrawCircle(Vector2.Zero, DotRadius, DotColor);
    }

    public void AdjustReticleLines()
    {
        var realVelocity = Player.GetRealVelocity();
        var origin = Vector3.Zero;
        var pos = Vector2.Zero;
        var speed = origin.DistanceTo(realVelocity);
        
        //top
        var topLine = ReticleLines.FirstOrDefault(line => line.Name == "Top");
        if (topLine is not null)
        {
            topLine.Position = topLine.Position.Lerp(pos + new Vector2(0, -speed - ReticleDistance), ReticleSpeed);
        }

        //right
        var rightLine = ReticleLines.FirstOrDefault(line => line.Name == "Right");
        if (rightLine is not null)
        {
            rightLine.Position = rightLine.Position.Lerp(pos + new Vector2(speed + ReticleDistance, 0), ReticleSpeed);
        }

        //bottom
        var bottomLine = ReticleLines.FirstOrDefault(line => line.Name == "Bottom");
        if (bottomLine is not null)
        {
            bottomLine.Position = bottomLine.Position.Lerp(pos + new Vector2(0, speed + ReticleDistance), ReticleSpeed);
        }

        //left
        var leftLine = ReticleLines.FirstOrDefault(line => line.Name == "Left");
        if (leftLine is not null)
        {
            leftLine.Position = leftLine.Position.Lerp(pos + new Vector2(-speed - ReticleDistance, 0), ReticleSpeed);
        }
    }
}
