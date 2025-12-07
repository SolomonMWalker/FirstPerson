using Godot;
using System.Collections.Generic;
using System.Linq;
using FirstPerson;

public partial class CoverSpotController : Node
{
    private double pollTimeInSeconds = 1;
    private List<CoverSpot> CoverSpots { get; set; } = [];

    public class TargetToDistanceOrderedCoverSpots (List<CoverSpot> coverSpots, double timeSinceLastPoll)
    {
        public List<CoverSpot> _coverSpots = coverSpots;
        public double _timeSinceLastPoll = timeSinceLastPoll;
    }

    private readonly Dictionary <ShootableCharacterBody3D, TargetToDistanceOrderedCoverSpots> targetToCoverSpots = [];
    private Player _player;

    public override void _Ready()
    {
        base._Ready();
        CoverSpots.AddRange(GetChildren().OfType<CoverSpot>());
        _player = GetNode<Player>("/root/Test/Player");
        targetToCoverSpots.Add(_player, 
            new TargetToDistanceOrderedCoverSpots(GetReorderedCoverSpots(_player), GD.Randf() * pollTimeInSeconds));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        RefreshTargetToCoverSpotsOrder(delta);
    }


    public CoverSpot GetViableCoverSpot(ShootableCharacterBody3D occupier, ShootableCharacterBody3D target,
        CoverSpot currentlyOccupiedSpot = null)
    {
        CalculateViabilityOfCoverSpots(target);
        var tempCoverSpots = FilterCoverSpots(targetToCoverSpots[target]._coverSpots, currentlyOccupiedSpot);
        CoverSpot bestCoverSpot = null;
        if(tempCoverSpots.Count != 0) 
            bestCoverSpot = tempCoverSpots.First();
        return bestCoverSpot;
    }

    private static bool IsCoverSpotViableAndUnoccupied(CoverSpot coverSpot) =>
        !coverSpot.occupied && coverSpot.isViable;
    
    private void CalculateViabilityOfCoverSpots(ShootableCharacterBody3D target)
    {
        if (!targetToCoverSpots.TryGetValue(target, out var value))
        {
            value = new TargetToDistanceOrderedCoverSpots(GetReorderedCoverSpots(target), 
                    GD.Randf() * pollTimeInSeconds);
            targetToCoverSpots.Add(target,value);
        }
        foreach (var coverSpot in value._coverSpots)
        {
            coverSpot.CalculateViability(target);
        }
    }

    private List<CoverSpot> GetReorderedCoverSpots(ShootableCharacterBody3D target)
    {
        return CoverSpots
            .OrderBy(c => c.GlobalPosition.DistanceSquaredTo(target.GlobalPosition))
            .ToList();
    }
    
    private List<CoverSpot> GetFilteredAndReorderedCoverSpots(Vector3 globalPosition, CoverSpot currentlyOccupiedSpot = null)
    {
        return CoverSpots
            .Where(c => IsCoverSpotViableAndUnoccupied(c) 
                || (currentlyOccupiedSpot is { isViable: true } && c == currentlyOccupiedSpot))
            .OrderBy(c => c.GlobalPosition.DistanceSquaredTo(globalPosition))
            .ToList();
    }

    private static List<CoverSpot> FilterCoverSpots(List<CoverSpot> coverSpots, CoverSpot currentlyOccupiedSpot = null)
    {
        return coverSpots
            .Where(c => IsCoverSpotViableAndUnoccupied(c)
                || (currentlyOccupiedSpot is { isViable: true } && c == currentlyOccupiedSpot))
            .ToList();
    }

    private void RefreshTargetToCoverSpotsOrder(double delta)
    {
        foreach (var key in targetToCoverSpots.Keys)
        {
            if (targetToCoverSpots[key]._timeSinceLastPoll > pollTimeInSeconds)
            {
                targetToCoverSpots[key]._coverSpots = GetReorderedCoverSpots(key);
            }
            else
            {
                targetToCoverSpots[key]._timeSinceLastPoll += delta;
            }
        }
    }
}
