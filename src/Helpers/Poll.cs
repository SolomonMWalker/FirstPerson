namespace FirstPerson.Helpers;

public class Poll
{
    public double PollTime { get; set; }
    public double TimeSincePoll { get; set; }
    
    public Poll(double pollTime)
    {
        PollTime = pollTime;
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
}