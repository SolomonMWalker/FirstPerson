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
    public List<CoverSpot> CoverSpots { get; private set; } = [];

    private Player _player;

    public override void _Ready()
    {
        base._Ready();
        CoverSpots.AddRange(GetChildren().OfType<CoverSpot>());
        _player = GetNode<Player>("/root/Test/Player");
    }
    
    
    public CoverSpot GetViableCoverSpot(ShootableCharacterBody3D occupier, ShootableCharacterBody3D target,
        CoverSpot currentlyOccupiedSpot = null)
    {
        CalculateViabilityOfCoverSpots(target);
        var tempCoverSpots = GetFilteredAndReorderedCoverSpots(target.GlobalPosition, currentlyOccupiedSpot);
        CoverSpot bestCoverSpot = null;
        if(tempCoverSpots.Count != 0) 
            bestCoverSpot = tempCoverSpots.First();

        if (bestCoverSpot == null) return null;
        //bestCoverSpot.Occupy(occupier);
        return bestCoverSpot;
    }

    private static bool IsCoverSpotViableAndUnoccupied(CoverSpot coverSpot) =>
        !coverSpot.occupied && coverSpot.isViable;
    
    private void CalculateViabilityOfCoverSpots(ShootableCharacterBody3D target)
        => CoverSpots.ForEach(cs => cs.CalculateViability(target));
    
    private List<CoverSpot> GetFilteredAndReorderedCoverSpots(Vector3 globalPosition, CoverSpot currentlyOccupiedSpot = null)
    {
        return CoverSpots
            .Where(c => IsCoverSpotViableAndUnoccupied(c) 
                        || (currentlyOccupiedSpot is { isViable: true } && c == currentlyOccupiedSpot))
            .OrderBy(c => c.GlobalPosition.DistanceSquaredTo(globalPosition))
            .ToList();
    }
}
