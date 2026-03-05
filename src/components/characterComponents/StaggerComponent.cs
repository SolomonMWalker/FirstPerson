using Godot;
using System;

[GlobalClass]
public partial class StaggerComponent : Node
{
    [Signal]
    public delegate void OnStaggerEventHandler();
    
    [Export] public Timer RechargeTimer { get; set; }
    [Export] public Timer NextStaggerTimer { get; set; }
    [Export] public float StartingAmount = 100f;
    [Export] public float TimeToFullRecharge { get; set; } = 1.5f;
    [Export] public float TimeBeforeRecharge { get; set; } = 2.0f;
    [Export] public float TimeBeforeAnotherStagger { get; set; } = 5.0f;

    public float CurrentAmount { get; private set; }
    public float CurrentMax { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        CurrentAmount = StartingAmount;
        CurrentMax = StartingAmount;
        RechargeTimer.WaitTime = TimeBeforeRecharge;
        NextStaggerTimer.WaitTime = TimeBeforeAnotherStagger;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (RechargeTimer.IsStopped())
        {
            Recharge(delta);
        }
    }

    public void SetStagger(int amount, bool setTotal)
    {
        if (setTotal)
        {
            CurrentMax = amount;
        }
        CurrentAmount = amount;
    }

    public void DepleteStagger(float staggerAmount)
    {
        RechargeTimer.Start();
        CurrentAmount -= staggerAmount;
        CurrentAmount = Mathf.Max(0, CurrentAmount);
        if (CurrentAmount == 0 && NextStaggerTimer.IsStopped())
        {
            EmitSignalOnStagger();
            NextStaggerTimer.Start();
        }
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
