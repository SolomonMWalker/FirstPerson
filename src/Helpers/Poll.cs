using System;
using Godot;

namespace FirstPerson.Helpers;

public class Poll(double pollTime, double initialTimeSincePoll = double.MaxValue)
{
    public double PollTime { get; set; } = pollTime;
    private double TimeSincePoll { get; set; } = initialTimeSincePoll > pollTime ? pollTime : initialTimeSincePoll;

    public bool IsPollPinged(double delta)
    {
        if (TimeSincePoll >= PollTime)
        {
            TimeSincePoll = 0;
            return true;
        }

        IncrementTime(delta);
        return false;
    }

    public void ResetPoll(double timeSince = 0) => TimeSincePoll = timeSince;

    public void AdvanceTimeWithoutPing(double delta) => IncrementTime(delta);

    public double GetPercentOfTimePassedInDecimal()
    {
        var fraction = TimeSincePoll / PollTime;
        if (Math.Abs(fraction - 1) < 0.001) fraction = 1;
        return fraction;
    }

    private void IncrementTime(double delta)
    {
        TimeSincePoll += delta;
        if (TimeSincePoll > PollTime) TimeSincePoll = PollTime;
    }
}