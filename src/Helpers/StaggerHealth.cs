using System;
using System.Collections.Generic;

namespace FirstPerson.Helpers;

public class StaggerHealth
{
    public int InitialAmount { get; private set; }
    public double Amount { get; private set; }
    public double PercentRegenPerSecond { get; private set; }
    private Poll TimeBeforeStaggerRegenPoll { get; set; }
    private Poll TimeBeforeAnotherStagger { get; set; }
    private bool IsRegainingStagger { get; set; }
    private bool WasStaggeredRecently { get; set; }

    public StaggerHealth(int initialAmount, double pauseBeforeRegenInSeconds = 2.5, 
        double percentRegenPerSecond = 50, double timeBeforeAnotherStagger = 6)
    {
        InitialAmount = initialAmount;
        Amount = initialAmount;
        TimeBeforeStaggerRegenPoll = new Poll(pauseBeforeRegenInSeconds);
        TimeBeforeAnotherStagger = new Poll(timeBeforeAnotherStagger);
        PercentRegenPerSecond = percentRegenPerSecond;
    }

    public void EndStagger()
    {
        Amount = InitialAmount;
    }

    public bool IsStaggeredFromDecreaseStaggerHealth(double amount)
    {
        if (WasStaggeredRecently) return false;
        
        Amount -= amount;
        IsRegainingStagger = false;
        
        if (Amount > 0)
        {
            TimeBeforeStaggerRegenPoll.ResetPoll();
            return false;
        }
        TimeBeforeAnotherStagger.ResetPoll();
        WasStaggeredRecently = true;
        Amount = InitialAmount;
        return true;
    }

    public void CheckStaggerRegain(double delta)
    {
        if (TimeBeforeAnotherStagger.IsPollPinged(delta))
        {
            WasStaggeredRecently = false;
        }
        if (WasStaggeredRecently) return;
        
        //If stagger amount is full, do nothing
        if (Math.Abs(Amount - InitialAmount) < 0.001)
        {
            IsRegainingStagger = false;
            return;
        }

        if (TimeBeforeStaggerRegenPoll.IsPollPinged(delta))
        {
            IsRegainingStagger = true;
        }

        if (!IsRegainingStagger) return;
        Amount += InitialAmount * (PercentRegenPerSecond / 100) * delta;
        if (Amount > InitialAmount) Amount = InitialAmount;
    }
}