namespace FirstPerson.Helpers;

public class Poll
{
    public double PollTime { get; set; }
    public double TimeSincePoll { get; set; }
    
    public Poll(double pollTime, double initialTimeSincePoll = double.MaxValue)
    {
        PollTime = pollTime;
        TimeSincePoll = initialTimeSincePoll;
    }

    public bool IsPollPinged(double delta)
    {
        if (TimeSincePoll > PollTime)
        {
            TimeSincePoll = 0;
            return true;
        }

        TimeSincePoll += delta;
        return false;
    }

    public void ResetPoll() => TimeSincePoll = 0;

    public void AdvanceTimeWithoutPing(double delta) => TimeSincePoll += delta;
}