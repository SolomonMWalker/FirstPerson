using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node
{
    [Signal]
    public delegate void OnDeathEventHandler();

    [Signal]
    public delegate void OnHealthDepletedEventHandler(float amount);

    [Export] public ShieldComponent ShieldComponent { get; private set; }
    [Export] public float StartingHealth { get; private set; } = 100;

    public float DamageMult { get; set; } = 1f;

    public float CurrentHealth { get; private set; }
    public float CurrentMax { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        CurrentHealth = StartingHealth;
        CurrentMax = StartingHealth;
    }

    public void SetHealth(float amount, bool setTotal)
    {
        if (setTotal)
        {
            CurrentMax = amount;
        }
        CurrentHealth = amount;
    }

    public void DepleteHealth(float amount)
    {
        if (ShieldComponent is not null && ShieldComponent.TryBlockWithShield(amount)) return;
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
        DepleteHealth(CurrentMax);
    }
}
