using Godot;
using System;

public partial class CoverSpot : Node3D
{
    public bool occupied = false;
    public ShootableCharacterBody3D occupier = null;
    public bool isViable => isTargetInAngleRange;

    private Node3D _player, _leftAngleDeterminant, _rightAngleDeterminant;
    private float _maxAngleInRads, _minAngleInRads;
    private bool isTargetInAngleRange;
    private bool isTargetInCoverRange;

    public override void _Ready()
    {
        base._Ready();
        _player = GetNode<Node3D>("/root/Test/Player");
        _leftAngleDeterminant = GetNode<Node3D>("LeftAngleDeterminant");
        _rightAngleDeterminant = GetNode<Node3D>("RightAngleDeterminant");
    
        _maxAngleInRads = Vector2.Up.AngleTo(new Vector2(_rightAngleDeterminant.Position.X, _rightAngleDeterminant.Position.Z));
        _minAngleInRads = Vector2.Up.AngleTo(new Vector2(_leftAngleDeterminant.Position.X, _leftAngleDeterminant.Position.Z));
    }
    
    public void CalculateViability(ShootableCharacterBody3D target)
    {
        CalculateIsTargetInAngleRange(target);
    }
    
    private void CalculateIsTargetInAngleRange(ShootableCharacterBody3D target)
    {
        var targetRelativePosition = ToLocal(target.GlobalPosition);
        var angleToTarget = Vector2.Up.AngleTo(new Vector2(targetRelativePosition.X, targetRelativePosition.Z));
        isTargetInAngleRange = angleToTarget >= _minAngleInRads && angleToTarget <= _maxAngleInRads;
    }
    
    
    public void Occupy(ShootableCharacterBody3D newOccupier)
    {
        occupier = newOccupier;
        occupied = true;
    }

    public void Unoccupy()
    {
        occupier = null;
        occupied = false;
    }
}
