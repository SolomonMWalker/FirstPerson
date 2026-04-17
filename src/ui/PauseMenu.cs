using Godot;
using System;

public partial class PauseMenu : Control
{
	[ExportCategory("Buttons")]
	[Export] public Button Resume { get; set; }
	[Export] public Button Restart { get; set; }
	[Export] public Button Quit { get; set; }
	
	[ExportCategory("Components")]
	[Export] public AnimationPlayer AnimationPlayer { get; set; }
	
	[Signal] public delegate void OnRestartGameEventHandler();
	[Signal] public delegate void OnQuitGameEventHandler();
	[Signal] public delegate void OnPauseGameEventHandler();
	[Signal] public delegate void OnUnpauseGameEventHandler();
	
	public bool IsPaused { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		WireUpButtons();
	}

	public void WireUpButtons()
	{
		Resume.Pressed += UnPauseGame;
		Restart.Pressed += EmitSignalOnRestartGame;
		Quit.Pressed += EmitSignalOnQuitGame;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (Input.IsActionJustPressed("Pause"))
		{
			if(IsPaused) UnPauseGame();
			else PauseGame();
		}
	}

	public void PauseGame()
	{
		GetTree().Paused = true;
		IsPaused = true;
		EmitSignalOnPauseGame();
		Input.MouseMode = Input.MouseModeEnum.Confined;
		AnimationPlayer.Play("blur");
		Show();
	}

	public void UnPauseGame()
	{
		GetTree().Paused = false;
		IsPaused = false;
		EmitSignalOnUnpauseGame();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		AnimationPlayer.PlayBackwards("blur");
		Hide();
	}
}
