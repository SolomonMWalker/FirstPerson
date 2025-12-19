using Godot;

namespace FirstPerson.Helpers;

//fully linear, might need to overcomplicate this
public class AccuracyController
{
    public Poll TimeSinceTargetMovedPoll { get; private set; }
    public double TimeTargetStoppedBeforeFullAccuracy { get; private set; } = 1;
    public bool TargetIsStopped { get; private set; }
    public bool AccuracyMaxed { get; private set; }
    public double ZeroVelocityThreshold { get; private set; } = 0.001f;
    //max difference in each axis TargetPosition of shot can vary
    public double MaxAccuracyVariance { get; private set; } = 2f;
    public double CurrentAccuracyVariance { get; private set; }

    public AccuracyController(double? timeToFullAccuracy = null)
    {
        TimeTargetStoppedBeforeFullAccuracy = timeToFullAccuracy ?? TimeTargetStoppedBeforeFullAccuracy;
        TimeSinceTargetMovedPoll = new Poll(TimeTargetStoppedBeforeFullAccuracy);
    }

    public void CheckTargetForAccuracy(double delta, CustomTypes.HittableCharacterBody3D target)
    {
        var targetIsStoppedNow = target is null || target.Velocity.LengthSquared() <= ZeroVelocityThreshold;
        
        switch (TargetIsStopped)
        {
            case true when !targetIsStoppedNow:
                //GD.Print("target went from stopped to moving");
                TargetIsStopped = false;
                AccuracyMaxed = false;
                CurrentAccuracyVariance = MaxAccuracyVariance;
                return;
            case true:
            {
                //GD.Print("hitting poll");
                if (!AccuracyMaxed && TimeSinceTargetMovedPoll.IsPollPinged(delta))
                {
                    //GD.Print("AccuracyMaxed");
                    AccuracyMaxed = true;
                }
                CurrentAccuracyVariance = AccuracyMaxed ? 0 :
                    MaxAccuracyVariance - TimeSinceTargetMovedPoll.GetPercentOfTimePassedInDecimal() * MaxAccuracyVariance;
                return;
            }
            case false when targetIsStoppedNow:
                //GD.Print("Target went from moving to stopped");
                TargetIsStopped = true;
                AccuracyMaxed = false;
                CurrentAccuracyVariance = MaxAccuracyVariance;
                TimeSinceTargetMovedPoll.ResetPoll();
                break;
        }
    }

    public Vector3 ApplyAccuracyToTargetPosition(Vector3 targetPosition)
    {
        return TargetIsStopped ? Fuzzer.Fuzz(targetPosition, (float) CurrentAccuracyVariance) 
            : Fuzzer.Fuzz(targetPosition, (float) MaxAccuracyVariance);
    }
        
}