using Godot;
using System;

public partial class OwLabel : Label
{
    [Export] public HealthComponent HealthComponent { get; set; }
    [Export] public Timer Timer { get; set; }

    public override void _Ready()
    {
        base._Ready();
        HealthComponent.OnHealthDepleted += amount =>
        {
            Activate();
            Timer.Start();
        };
        Timer.Timeout += Deactivate;
    }

    private void Activate() => Visible = true;
    private void Deactivate() => Visible = false;
}
