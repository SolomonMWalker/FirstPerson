using Godot;
using System;
using FirstPerson.environment.utilities;

public partial class DisappearingWall : StaticBody3D
{
    [Export] public EncounterZone EncounterZone { get; set; }

    public override void _Ready()
    {
        base._Ready();
        EncounterZone.OnEncounterZoneDone += () =>
        {
            GD.Print("Disappearing!");
            QueueFree();
        };
    }
    
}
