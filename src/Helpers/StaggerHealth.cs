using System;
using System.Collections.Generic;

namespace FirstPerson.Helpers;

public class StaggerHealth
{
    public int InitialAmount { get; private set; }
    public double Amount { get; private set; }
    public double PauseBeforeRegenInSeconds { get; private set; }
    public double PercentRegenPerSecond { get; private set; }
    private double TimeSinceStaggerDecrease { get; set; }
    private bool IsStaggered { get; set; }

    public StaggerHealth(int initialAmount, double pauseBeforeRegenInSeconds = 2.5, 
        double percentRegenPerSecond = 50)
    {
        InitialAmount = initialAmount;
        Amount = initialAmount;
        PauseBeforeRegenInSeconds = pauseBeforeRegenInSeconds;
        PercentRegenPerSecond = percentRegenPerSecond;
    }

    public void EndStagger()
    {
        Amount = InitialAmount;
    }

    public bool IsStaggeredFromDecreaseStaggerHealth(double amount)
    {
        Amount -= amount;
        TimeSinceStaggerDecrease = 0;
        if (Amount > 0)
        {
            IsStaggered = true;
            return false;
        }
        Amount = InitialAmount;
        return true;
    }

    public void CheckStaggerRegain(double delta)
    {
        //If stagger amount is full, do nothing
        if (Math.Abs(Amount - InitialAmount) < 0.001)
        {
            return;
        }
        if (TimeSinceStaggerDecrease > PauseBeforeRegenInSeconds)
        {
            Amount += InitialAmount * (PercentRegenPerSecond / 100) * delta;
            if (Amount > InitialAmount) Amount = InitialAmount;
        }
        else
        {
            TimeSinceStaggerDecrease += delta;
        }
    }
}