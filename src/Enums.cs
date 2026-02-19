namespace FirstPerson;

public enum Goal
{
    MoveToCover,
    MoveToSpot,
    MoveToTarget,
    Patrol,
    Standby
}

public enum AgentMovementState
{
    Still,
    DefaultMoving
}