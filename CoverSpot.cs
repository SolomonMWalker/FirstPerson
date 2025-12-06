using Godot;
using System;

public partial class CoverSpot : Node3D
{
    public bool occupied = false;
    public ShootableCharacterBody3D occupier = null;
    public bool playerInAngleRange;
    public bool playerInCoverRange = true;
    public double playerPollTime = 0.3;

    private Node3D _player, _leftAngleDeterminant, _rightAngleDeterminant;
    private float _maxAngleInRads, _minAngleInRads;
    private float? _coverToPlayerMaxDistance;
    private double _timeSinceLastPoll = 2;

    public override void _Ready()
    {
        base._Ready();
        _player = GetNode<Node3D>("/root/Test/Player");
        _leftAngleDeterminant = GetNode<Node3D>("LeftAngleDeterminant");
        _rightAngleDeterminant = GetNode<Node3D>("RightAngleDeterminant");
    
        _maxAngleInRads = Vector2.Up.AngleTo(new Vector2(_rightAngleDeterminant.Position.X, _rightAngleDeterminant.Position.Z));
        _minAngleInRads = Vector2.Up.AngleTo(new Vector2(_leftAngleDeterminant.Position.X, _leftAngleDeterminant.Position.Z));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_timeSinceLastPoll > playerPollTime)
        {
            _timeSinceLastPoll = 0;
            playerInAngleRange = IsPlayerInAngleRange();
            playerInCoverRange = IsPlayerInCoverRange();
        }
        else
        {
            _timeSinceLastPoll += delta;
        }
    }

    public bool IsViable(float? coverToPlayerMaxDistance = null)
    {
        if (coverToPlayerMaxDistance == null) return IsPlayerInAngleRange();
        return GlobalPosition.DistanceTo(_player.GlobalPosition) <= coverToPlayerMaxDistance && IsPlayerInAngleRange();
    }

    private bool IsPlayerInAngleRange()
    {
        var playerRelativePosition = ToLocal(_player.GlobalPosition);
        var angleToPlayer = Vector2.Up.AngleTo(new Vector2(playerRelativePosition.X, playerRelativePosition.Z));
        return angleToPlayer >= _minAngleInRads && angleToPlayer <= _maxAngleInRads;
    }

    private bool IsPlayerInCoverRange() => GlobalPosition.DistanceTo(_player.GlobalPosition) <= _coverToPlayerMaxDistance;
    
    public void Occupy(ShootableCharacterBody3D newOccupier, float? coverToPlayerMaxDistance = null)
    {
        occupier = newOccupier;
        occupied = true;
        _coverToPlayerMaxDistance = coverToPlayerMaxDistance;
    }

    public void Unoccupy()
    {
        occupier = null;
        occupied = false;
        _coverToPlayerMaxDistance = null;
        playerInCoverRange = true;
    }
}
