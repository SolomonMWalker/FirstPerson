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
        _player = GetNode<Player>("/root/Test/Player");
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

    public CoverSpot GetAndOccupyViableCoverSpot(ShootableCharacterBody3D occupier,
        CoverSpot currentlyOccupiedSpot = null, float? coverToPlayerMaxDistance = null)
    {
        var bestCoverSpot = GetFirstViableAndUnoccupiedCoverSpot();
        if (bestCoverSpot == null)
        {
            if (currentlyOccupiedSpot != null) bestCoverSpot = currentlyOccupiedSpot;
            else return null;
        }
        
        if (currentlyOccupiedSpot != null)
        {
            if (CoverSpots.IndexOf(currentlyOccupiedSpot) < CoverSpots.IndexOf(bestCoverSpot))
            {
                bestCoverSpot = currentlyOccupiedSpot;
            }
        }

        if (!bestCoverSpot.IsViable(coverToPlayerMaxDistance)) return null;
        bestCoverSpot.Occupy(occupier, coverToPlayerMaxDistance);
        return bestCoverSpot;
    }

    private void UnoccupyCoverSpot(ShootableCharacterBody3D occupier)
    {
        if (!TryGetCoverSpotOccupiedBy(occupier, out var spot)) return;
        spot.Unoccupy();
    }

    private static bool IsCoverSpotViableAndUnoccupied(CoverSpot coverSpot) =>
        !coverSpot.occupied && coverSpot.playerInAngleRange && coverSpot.playerInAngleRange;
    
    private static bool IsCoverSpotViable(CoverSpot coverSpot) =>
        !coverSpot.occupied && coverSpot.playerInAngleRange && coverSpot.playerInAngleRange;
    
    private CoverSpot GetFirstViableAndUnoccupiedCoverSpot() => CoverSpots.FirstOrDefault(IsCoverSpotViableAndUnoccupied);
    private CoverSpot GetFirstViableCoverSpot() => CoverSpots.FirstOrDefault(IsCoverSpotViable);

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
