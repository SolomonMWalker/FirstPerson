using Godot;
using System;

public partial class CoverSpot : Node3D
{
    public bool occupied = false;
    public ShootableCharacterBody3D occupier = null;
    public bool playerInAngleRange;
    public double playerPollTime = 0.3;

    private Node3D _player, _leftAngleDeterminant, _rightAngleDeterminant;
    private float _maxAngleInRads, _minAngleInRads;
    private double _timeSinceLastPoll = 2;

    public override void _Ready()
    {
        base._Ready();
        _player = GetNode<Node3D>("../../Player");
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
            
            var playerRelativePosition = ToLocal(_player.GlobalPosition);
            var angleToPlayer = Vector2.Up.AngleTo(new Vector2(playerRelativePosition.X, playerRelativePosition.Z));
            if (angleToPlayer >= _minAngleInRads && angleToPlayer <= _maxAngleInRads)
            {
                if (!playerInAngleRange) playerInAngleRange = true;
            }
            else if (playerInAngleRange)
            {
                playerInAngleRange = false;
            }
        }
        else
        {
            _timeSinceLastPoll += delta;
        }
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
