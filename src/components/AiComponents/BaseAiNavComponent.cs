using Godot;

namespace FirstPerson.agents.AiComponents;

public abstract partial class BaseAiNavComponent : Node
{
    [ExportCategory("References")]
    [Export] public NavigationAgent3D NavigationAgent3D { get; private set; }
    [Export] public Grunt Grunt { get; private set; }

    public abstract void HandleNavigation(double delta);
}