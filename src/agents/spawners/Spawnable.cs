using Godot;

namespace FirstPerson.agents.spawners;

[GlobalClass]
public partial class Spawnable : Resource
{
    [Export] public PackedScene ToSpawn { get; set; }
    [Export] public SpawnableType Type { get; set; }
    [Export] public float HeightFromGround { get; set; }

    public enum SpawnableType
    {
        grunt
    }
}