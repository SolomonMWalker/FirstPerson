using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node
{
    [Signal]
    public delegate void OnDeathEventHandler();

    [Signal]
    public delegate void OnHealthDepletedEventHandler(int amount);

    [Export] public int StartingHealth { get; private set; } = 100;

    public int CurrentHealth { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        CurrentHealth = StartingHealth;
    }

    public void SetHealth(int amount, bool setTotal)
    {
        if (setTotal)
        {
            StartingHealth = amount;
        }
        CurrentHealth = amount;
    }

    public void DepleteHealth(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            EmitSignal(SignalName.OnDeath);
        }
        else
        {
            EmitSignal(SignalName.OnHealthDepleted, amount);
        }
    }

    public void Kill()
    {
        DepleteHealth(StartingHealth);
    }
}
