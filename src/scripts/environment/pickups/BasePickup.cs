using Godot;
using FirstPerson.Scenes.Player;

public partial class BasePickup : Area3D
{
    [Export] public float RotationSpeed = 60f;
    [Export] public float FloatHeight = 0.1f;
    [Export] public float FloatSpeed = 2.0f;
    
    private float StartY { get; set; }
    private float Time { get; set; }

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnPickup;
        StartY = Position.Y;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        var fDelta = (float)delta;
        
        //bobbing animation
        Time += fDelta;
        var newY = StartY + Mathf.Sin(Time * FloatSpeed) * FloatHeight;
        Position = Position with { Y = newY };
        
        //optional rotation animation
        RotateY(Mathf.DegToRad(RotationSpeed) * fDelta);
    }

    public void OnPickup(Node3D body)
    {
        if (body is not PlayerController player) return;

        if (CanPickup(player))
        {
            ApplyPickup(player);
            QueueFree();
        }
    }

    public virtual bool CanPickup(PlayerController player) => true;

    public virtual void ApplyPickup(PlayerController player) {}
}
