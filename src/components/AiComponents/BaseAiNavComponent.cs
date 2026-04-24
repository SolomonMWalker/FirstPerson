using FirstPerson.agents.enemies.test;
using Godot;

namespace FirstPerson.agents.AiComponents;

public abstract partial class BaseAiNavComponent : Node
{
    [ExportCategory("References")]
    [Export] public NavigationAgent3D NavigationAgent3D { get; private set; }
    [Export] public MovingAgent MovingAgent { get; private set; }

    public abstract void HandleNavigation(double delta);

    protected bool HasNavMap()
    {
        return NavigationServer3D.MapGetIterationId(NavigationAgent3D.GetNavigationMap()) != 0;
    }
}