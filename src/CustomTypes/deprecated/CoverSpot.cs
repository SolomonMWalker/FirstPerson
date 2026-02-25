using FirstPerson.CustomTypes;
using Godot;

public partial class CoverSpot : Node3D
{
    public bool Occupied { get; private set; }
    public HittableCharacterBody3D Occupier { get; private set; }
    public bool IsViable => IsTargetInAngleRange;

    private Node3D Player { get; set; }
    private Node3D LeftAngleDeterminant { get; set; }
    private Node3D RightAngleDeterminant { get; set; }
    private float MaxAngleInRads { get; set; }
    private float MinAngleInRads { get; set; }
    private bool IsTargetInAngleRange { get; set; }
    private bool IsTargetInCoverRange { get; set; }

    public override void _Ready()
    {
        base._Ready();
        Player = GetNode<Node3D>("/root/Test/Player");
        LeftAngleDeterminant = GetNode<Node3D>("LeftAngleDeterminant");
        RightAngleDeterminant = GetNode<Node3D>("RightAngleDeterminant");
    
        MaxAngleInRads = Vector2.Up.AngleTo(new Vector2(RightAngleDeterminant.Position.X, RightAngleDeterminant.Position.Z));
        MinAngleInRads = Vector2.Up.AngleTo(new Vector2(LeftAngleDeterminant.Position.X, LeftAngleDeterminant.Position.Z));
    }
    
    public void CalculateViability(HittableCharacterBody3D target)
    {
        CalculateIsTargetInAngleRange(target);
    }
    
    private void CalculateIsTargetInAngleRange(HittableCharacterBody3D target)
    {
        var targetRelativePosition = ToLocal(target.GlobalPosition);
        var angleToTarget = Vector2.Up.AngleTo(new Vector2(targetRelativePosition.X, targetRelativePosition.Z));
        IsTargetInAngleRange = angleToTarget >= MinAngleInRads && angleToTarget <= MaxAngleInRads;
    }
    
    
    public void Occupy(HittableCharacterBody3D newOccupier)
    {
        Occupier = newOccupier;
        Occupied = true;
    }

    public void Unoccupy()
    {
        Occupier = null;
        Occupied = false;
    }
}
