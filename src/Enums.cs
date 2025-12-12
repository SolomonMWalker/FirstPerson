namespace FirstPerson;

public enum Goal
{
    MoveToCover,
    MoveToTargetClose,
    MoveToTargetMedium,
    MoveToTargetFar,
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