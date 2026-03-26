using System.Linq;
using FirstPerson.agents.enemies.test;
using FirstPerson.scenes.enemies.test;
using Godot;
using EncounterZone = FirstPerson.environment.utilities.EncounterZone;

namespace FirstPerson.agents.spawners;

public partial class Spawner : Node3D
{
    [Export] public Spawnable.SpawnableType SpawnableType;
    [Export] public EncounterZone EncounterZone;
    
    [ExportCategory("References")]
    [Export] public SpawnableCollection SpawnableCollection;
    [Export] public Node3D SpawnLocationNode;
    [Export] public Node3D ParentOfSpawned;


    private Spawnable _currentSpawnable;

    public override void _Ready()
    {
        base._Ready();
        _currentSpawnable = SpawnableCollection.SpawnableList.ToList()
            .FirstOrDefault(spawn => spawn.Type == SpawnableType);
        if (_currentSpawnable == null) return;
        SpawnLocationNode.Position = new Vector3(0, _currentSpawnable.HeightFromGround, 0);
    }

    public void Spawn()
    {
        if (_currentSpawnable is null) return;
        var instance = _currentSpawnable.ToSpawn.Instantiate<MovingAgent>();
        AddChild(instance);
        instance.Reparent(ParentOfSpawned);
        instance.PostSpawnInitialize(EncounterZone);
    }
}