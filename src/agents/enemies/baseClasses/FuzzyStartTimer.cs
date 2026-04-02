using Godot;
using System;

public partial class FuzzyStartTimer : Timer
{
    private Random _random;
    private double _accessibleWaitTime;
    private bool _started;

    public override void _Ready()
    {
        base._Ready();
        _random = new Random();
    }

    public void SetStartTime(float time)
    {
        _accessibleWaitTime = time;
        WaitTime = time;
    } 

    public void FuzzyStart()
    {
        if (!_started)
        {
            Start(_random.NextDouble() * _accessibleWaitTime);
            _started = true;
        }
        else
        {
            Start(_accessibleWaitTime);
        }
    }
}
