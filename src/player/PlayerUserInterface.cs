using Godot;
using FirstPerson.scripts.system;

public partial class PlayerUserInterface : Control
{
    public override void _Ready()
    {
        base._Ready();
        var gameManager = (GameManager) GetTree().GetFirstNodeInGroup("gameManager");
        gameManager.PauseMenu.OnPauseGame += Hide;
        gameManager.PauseMenu.OnUnpauseGame += Show;
    }
}
