using Godot;

namespace FirstPerson.Helpers;

public class Poll(double pollTime, double initialTimeSincePoll = double.MaxValue)
{
    private double PollTime { get; set; } = pollTime;
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

    public void ResetPoll(double newStartTime = 0) => TimeSincePoll = newStartTime;

    public void AdvanceTimeWithoutPing(double delta) => IncrementTime(delta);

    private void IncrementTime(double delta)
    {
        TimeSincePoll += delta;
        if (TimeSincePoll > PollTime) TimeSincePoll = PollTime;
    }

}