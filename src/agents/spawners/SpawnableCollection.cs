using Godot;

namespace FirstPerson.agents.spawners;

[GlobalClass]
public partial class SpawnableCollection: Resource
{
    [Export] public Godot.Collections.Array<spawners.Spawnable> SpawnableList;
}