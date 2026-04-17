using Godot;

namespace FirstPerson.scripts.system;

public partial class GameManager : Node
{
    [Export] public PauseMenu PauseMenu { get; set; }
    
    public bool IsPaused { get; set; }

    public override void _Ready()
    {
        base._Ready();
        WireUpSignals();
    }

    public void WireUpSignals()
    {
        PauseMenu.OnRestartGame += () =>
        {
            PauseMenu.UnPauseGame();
            RestartGame();
        };
        PauseMenu.OnQuitGame += QuitGame;
        PauseMenu.OnPauseGame += () =>
        {
            IsPaused = true;
        };
        PauseMenu.OnUnpauseGame += () =>
        {
            IsPaused = false;
        };
    }

    public void QuitGame()
    {
        GetTree().Quit();
    }

    public void RestartGame()
    {
        GetTree().ReloadCurrentScene();
    }
}