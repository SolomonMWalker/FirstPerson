using Godot;

namespace FirstPerson.agents.enemies;

[GlobalClass]
public partial class SpawnableCollection: Resource
{
    [Export] public Godot.Collections.Array<Spawnable> SpawnableList;
}