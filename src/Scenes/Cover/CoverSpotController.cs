using Godot;
using System.Collections.Generic;
using System.Linq;
using FirstPerson.Configuration;
using Player = FirstPerson.Scenes.Player.Player;

public partial class CoverSpotController : Node
{
    private double PollTimeInSeconds { get; set; } = 1;
    private List<CoverSpot> CoverSpots { get; set; } = [];

    public class TargetToDistanceOrderedCoverSpots (List<CoverSpot> coverSpots, double timeSinceLastPoll)
    {
        public List<CoverSpot> CoverSpots { get; set; } = coverSpots;
        public double TimeSinceLastPoll { get; set; } = timeSinceLastPoll;
    }

    private Dictionary <FirstPerson.CustomTypes.HittableCharacterBody3D, TargetToDistanceOrderedCoverSpots> 
        TargetToCoverSpots { get; set; } = [];
    private Player _player;

    public override void _Ready()
    {
        base._Ready();
        CoverSpots.AddRange(GetChildren().OfType<CoverSpot>());
        _player = GetNode<Player>(Configuration.GetConfigValues().PlayerSceneTreePath);
        TargetToCoverSpots.Add(_player, 
            new TargetToDistanceOrderedCoverSpots(GetReorderedCoverSpots(_player), GD.Randf() * PollTimeInSeconds));
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        RefreshTargetToCoverSpotsOrder(delta);
    }


    public CoverSpot GetViableCoverSpot(FirstPerson.CustomTypes.HittableCharacterBody3D occupier, FirstPerson.CustomTypes.HittableCharacterBody3D target,
        CoverSpot currentlyOccupiedSpot = null)
    {
        CalculateViabilityOfCoverSpots(target);
        var tempCoverSpots = FilterCoverSpots(TargetToCoverSpots[target].CoverSpots, currentlyOccupiedSpot);
        CoverSpot bestCoverSpot = null;
        if(tempCoverSpots.Count != 0) 
            bestCoverSpot = tempCoverSpots.First();
        return bestCoverSpot;
    }

    private static bool IsCoverSpotViableAndUnoccupied(CoverSpot coverSpot) =>
        !coverSpot.Occupied && coverSpot.IsViable;
    
    private void CalculateViabilityOfCoverSpots(FirstPerson.CustomTypes.HittableCharacterBody3D target)
    {
        if (!TargetToCoverSpots.TryGetValue(target, out var value))
        {
            value = new TargetToDistanceOrderedCoverSpots(GetReorderedCoverSpots(target), 
                    GD.Randf() * PollTimeInSeconds);
            TargetToCoverSpots.Add(target,value);
        }
        foreach (var coverSpot in value.CoverSpots)
        {
            coverSpot.CalculateViability(target);
        }
    }

    private List<CoverSpot> GetReorderedCoverSpots(FirstPerson.CustomTypes.HittableCharacterBody3D target)
    {
        return CoverSpots
            .OrderBy(c => c.GlobalPosition.DistanceSquaredTo(target.GlobalPosition))
            .ToList();
    }

    private static List<CoverSpot> FilterCoverSpots(List<CoverSpot> coverSpots, CoverSpot currentlyOccupiedSpot = null)
    {
        return coverSpots
            .Where(c => IsCoverSpotViableAndUnoccupied(c)
                || (currentlyOccupiedSpot is { IsViable: true } && c == currentlyOccupiedSpot))
            .ToList();
    }

    private void RefreshTargetToCoverSpotsOrder(double delta)
    {
        foreach (var key in TargetToCoverSpots.Keys)
        {
            if (TargetToCoverSpots[key].TimeSinceLastPoll > PollTimeInSeconds)
            {
                TargetToCoverSpots[key].CoverSpots = GetReorderedCoverSpots(key);
            }
            else
            {
                TargetToCoverSpots[key].TimeSinceLastPoll += delta;
            }
        }
    }
}
