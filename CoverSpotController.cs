using Godot;
using System.Collections.Generic;
using System.Linq;
using FirstPerson;

public partial class CoverSpotController : Node
{
    //if I need to implement this for multiple cover groups
    //ex. ally troops taking cover from enemies we're all fighting
    //make CoverSpots a dictionary where each list is reordered by the thing to take cover from
    //could get cumbersome, maybe each ally picks an enemy to shoot and cover is based off of that
    //or group, not sure
    //future problem
    [Export] public float ReorderPolltime = 0.5f;
    public List<CoverSpot> CoverSpots { get; private set; } = [];

    private Player _player;
    private double _timeSinceLastPoll = 5;

    public override void _Ready()
    {
        base._Ready();
        CoverSpots.AddRange(GetChildren().OfType<CoverSpot>());
        _player = GetNode<Player>("../Player");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_timeSinceLastPoll > ReorderPolltime)
        {
            ReorderCoverSpotListBasedOnPlayerPosition();
            _timeSinceLastPoll = 0;
        }
        else
        {
            _timeSinceLastPoll += delta;
        }
    }

    public CoverSpot GetAndOccupyViableCoverSpot(ShootableCharacterBody3D occupier)
    {
        CoverSpot occupiedCoverSpot = null;
        if (TryGetCoverSpotOccupiedBy(occupier, out var coverSpot) && coverSpot.playerInAngleRange)
        {
            occupiedCoverSpot = coverSpot;
        }

        var firstViableCoverSpot = GetFirstViableCoverSpot();
        if (firstViableCoverSpot == null) return occupiedCoverSpot;
        if (occupiedCoverSpot != null)
        {
            //firstUnoccCoverSpot is closer than occupiedCoverSpot
            if (CoverSpots.IndexOf(firstViableCoverSpot) < CoverSpots.IndexOf(occupiedCoverSpot))
            {
                //unoccupy the currently occupied spot
                occupiedCoverSpot.Unoccupy();
            }
            else
            {
                //no change
                return occupiedCoverSpot;
            }
        }
        
        //if the occupied spot isn't the closest, grab first open spot
        firstViableCoverSpot.Occupy(occupier);
        return firstViableCoverSpot;
    }
    

    public void UnoccupyCoverSpot(ShootableCharacterBody3D occupier)
    {
        if (!TryGetCoverSpotOccupiedBy(occupier, out var spot)) return;
        spot.Unoccupy();
    }

    private CoverSpot GetFirstViableCoverSpot() => CoverSpots.FirstOrDefault(cs => !cs.occupied && cs.playerInAngleRange);

    private bool TryGetCoverSpotOccupiedBy(ShootableCharacterBody3D occupier, out CoverSpot coverSpot)
    {
        coverSpot = null;
        if(CoverSpots.All(cs => cs.occupier != occupier)) return false;
        coverSpot = CoverSpots.FirstOrDefault(cs => cs.occupier == occupier);
        return true;
    }

    private void ReorderCoverSpotListBasedOnPlayerPosition() =>
        ReorderCoverSpotListBasedOnGlobalPosition(_player.GlobalPosition);

    private void ReorderCoverSpotListBasedOnGlobalPosition(Vector3 globalPosition)
    {
        CoverSpots = CoverSpots
            .OrderBy(c => c.GlobalPosition.DistanceSquaredTo(globalPosition))
            .ToList();
    }
}
