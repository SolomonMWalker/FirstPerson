using Godot;
using System;

public partial class ShieldComponent : Node
{
    [Export] public Timer RechargeTimer { get; set; }
    [Export] public float TimeToFullRecharge { get; set; } = 1.5f;
    [Export] public float TimeBeforeRecharge { get; set; } = 2.0f;
    [Export] public float StartingAmount = 100f;
    
    public float CurrentAmount { get; private set; }
    public float CurrentMax { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        CurrentAmount = StartingAmount;
        CurrentMax = StartingAmount;
        RechargeTimer.WaitTime = TimeBeforeRecharge;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (RechargeTimer.IsStopped())
        {
            Recharge(delta);
        }
    }

    public bool TryBlockWithShield(float damageAmount)
    {
        RechargeTimer.Start();
        if (CurrentAmount == 0) return false;
        CurrentAmount -= damageAmount;
        CurrentAmount = Mathf.Max(0, CurrentAmount);
        return true;
    }

    private void Recharge(double delta)
    {
        if (Math.Abs(CurrentAmount - CurrentMax) < 0.01f)
        {
            CurrentAmount = CurrentMax;
            return;
        }
        var rechargePerSecond = CurrentMax / TimeToFullRecharge;
        CurrentAmount = Mathf.Min(CurrentAmount + rechargePerSecond * (float)delta, CurrentMax);
    }
}
