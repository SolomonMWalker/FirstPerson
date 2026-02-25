using Godot;

namespace FirstPerson.scripts.system;

public partial class GameManager : Node
{
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledKeyInput(@event);
        if (@event.IsActionPressed("DevExit"))
        {
            GetTree().Quit();
            return;
        }

        if (@event.IsActionPressed("DevReload"))
        {
            GetTree().ReloadCurrentScene();
            return;
        }
    }
}