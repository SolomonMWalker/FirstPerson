namespace FirstPerson.Helpers;

public enum Goal
{
    MoveToCover,
    MoveToTarget,
    Patrol,
    Standby
}

public enum AgentMovementState
{
    Still,
    DefaultMoving
}