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

public enum PlayerMovementState
{
    Default,
    Crouching,
    Sprinting
}

public enum PlayerActionState
{
    OnFloor,
    InAir,
    Clambering,
    CoyoteTime
}